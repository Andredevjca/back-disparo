using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DisparoApi.Options;
using DisparoApi.Data;
using DisparoApi.Repositories;
using DisparoApi.Services;
using DisparoApi.Dtos;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

var contentRoot = builder.Environment.ContentRootPath;
var binDir = AppContext.BaseDirectory;
builder.Configuration
    .SetBasePath(contentRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddJsonFile(Path.Combine(binDir, "appsettings.json"), optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.Configure<DefaultsOptions>(
    builder.Configuration.GetSection("Defaults"));

var defaults = builder.Configuration.GetSection("Defaults").Get<DefaultsOptions>() ?? new DefaultsOptions();
if (defaults.IntervaloMs <= 0) defaults.IntervaloMs = 3000;

var mysqlCs = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
if (string.IsNullOrWhiteSpace(mysqlCs))
{
    var arqUsado = Path.Combine(contentRoot, "appsettings.json");
    throw new InvalidOperationException(
        "ConnectionString do MySQL ausente. " + Environment.NewLine +
        $"Edite: {arqUsado}" + Environment.NewLine +
        "Preencha ConnectionStrings > DefaultConnection.");
}

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Chave JWT ausente. Edite appsettings.json: Jwt > Secret (mínimo 16 caracteres).");
if (jwtSecret.Length < 16)
    throw new InvalidOperationException($"Chave JWT muito curta ({jwtSecret.Length} caracteres). Mínimo de 16.");
var jwtExpireHoursRaw = builder.Configuration["Jwt:ExpireHours"];
var jwtExpireHours = int.TryParse(jwtExpireHoursRaw, out var eh) && eh > 0 ? eh : 12;
builder.Services.Configure<JwtOptions>(o =>
{
    o.Secret = jwtSecret;
    o.ExpireHours = jwtExpireHours;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var msg = context.AuthenticateFailure != null ? "Sessão inválida" : "Não autenticado";
                return context.Response.WriteAsJsonAsync(new { message = msg });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { message = "Acesso negado" });
            }
        };
    });

var cors = builder.Configuration.GetSection("Cors").Get<CorsOptions>() ?? new CorsOptions();
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection("Cors"));
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            if (cors.Origins != null && cors.Origins.Length > 0)
                policy.WithOrigins(cors.Origins).AllowCredentials().AllowAnyHeader().AllowAnyMethod();
            else
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnectionFactory>(_ => new DbConnectionFactory(mysqlCs));
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IConfiguracaoRepository, ConfiguracaoRepository>();
builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();
builder.Services.AddScoped<IImportacaoRepository, ImportacaoRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEnvioService, EnvioService>();
builder.Services.AddScoped<IImportacaoService, ImportacaoService>();
builder.Services.AddScoped<IAtendimentoRepository, AtendimentoRepository>();
builder.Services.AddScoped<IAtendimentoService, AtendimentoService>();
builder.Services.AddSingleton<SincroniaMonitor>();
builder.Services.AddSingleton<IEnvioMassaJobManager, EnvioMassaJobManager>();
builder.Services.AddHttpClient<IEvolutionApiService, EvolutionApiService>();
builder.Services.AddAuthorization();

var evoKey = builder.Configuration["Evolution:ApiKey"];
var evoUrl = builder.Configuration["Evolution:Url"];
var evoInstance = builder.Configuration["Evolution:Instance"];
builder.Services.Configure<EvolutionOptions>(o =>
{
    o.Url = string.IsNullOrWhiteSpace(evoUrl) ? "http://localhost:8080" : evoUrl.TrimEnd('/');
    o.ApiKey = evoKey ?? string.Empty;
    o.Instance = string.IsNullOrWhiteSpace(evoInstance) ? "meu-whatsapp" : evoInstance;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();

    await using var conn = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().Create();
    await conn.OpenAsync();
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = "UPDATE envios SET status = 'PAUSADO' WHERE status IN ('EM_ANDAMENTO','PAUSADO')";
    await cmd.ExecuteNonQueryAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Disparo API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
