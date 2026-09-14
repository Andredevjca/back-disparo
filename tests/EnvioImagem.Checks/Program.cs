using System.Reflection;
using DisparoApi.Repositories;
using System.Net;
using System.Text;
using System.Text.Json;
using DisparoApi.Dtos;
using DisparoApi.Options;
using DisparoApi.Services;
using Microsoft.Extensions.Options;

void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
void Invalid(ImagemEnvioDto image) {
    try { image.Validar(); throw new Exception("Imagem inválida aceita"); }
    catch (ArgumentException) { }
}
var image = new ImagemEnvioDto { Base64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aB9sAAAAASUVORK5CYII=", MimeType = "image/png" };
image.Validar();
Invalid(new() { Base64 = "%%%", MimeType = "image/png" });
Invalid(new() { Base64 = image.Base64, MimeType = "image/jpeg" });
Invalid(new() { Base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("texto")), MimeType = "image/png" });
Invalid(new() { Base64 = new string('A', 6990509), MimeType = "image/png" });
Invalid(new() { Base64 = null!, MimeType = "image/png" });
var handler = new CaptureHandler();
var service = new EvolutionApiService(new HttpClient(handler), Options.Create(new EvolutionOptions { Url = "https://example.test", ApiKey = "test" }));
var result = await service.EnviarMensagemAsync("conta", "5585999999999", "Olá Maria", image);
Check(result.ok && result.evolutionId == "abc", "Resultado da mídia");
Check(handler.Path == "/message/sendMedia/conta", "Endpoint de mídia");
using (var json = JsonDocument.Parse(handler.Body)) {
    var body = json.RootElement;
    Check(body.GetProperty("caption").GetString() == "Olá Maria", "Legenda personalizada");
    Check(body.GetProperty("media").GetString() == image.Base64, "Conteúdo da imagem");
    Check(body.GetProperty("mimetype").GetString() == "image/png", "Tipo da imagem");
    Check(body.GetProperty("mediatype").GetString() == "image", "Tipo de mídia");
}
await service.EnviarMensagemAsync("conta", "5585999999999", "Texto");
Check(handler.Path == "/message/sendText/conta", "Regressão do envio de texto");
handler.Status = HttpStatusCode.BadRequest;
result = await service.EnviarMensagemAsync("conta", "5585999999999", "", image);
Check(!result.ok && result.erro != null, "Falha da Evolution deve ser retornada");

// Exercise all dispatch entry points without accessing MySQL or sending real messages.
ImagemEnvioDto? saved = null;
var template = new TemplateResponse { Id = 1, Nome = "Modelo", Mensagem = "Olá {{nome}}", Imagem = image };
var replacement = new ImagemEnvioDto { Base64 = image.Base64, MimeType = image.MimeType };
T Stub<T>(Func<MethodInfo, object?[], object?> call) where T : class {
    var proxy = DispatchProxy.Create<T, TestProxy>();
    ((TestProxy)(object)proxy).Call = call;
    return proxy;
}
var repository = Stub<IEnvioRepository>((m, a) => {
    if (m.Name == "SalvarImagemAsync") { saved = (ImagemEnvioDto)a[1]!; return Task.CompletedTask; }
    if (m.Name == "CriarEnvioUnitarioAsync") return Task.FromResult((1, 2));
    if (m.Name == "CriarEnvioMassaCabecalhoAsync") return Task.FromResult(1);
    if (m.ReturnType == typeof(Task)) return Task.CompletedTask;
    throw new Exception("Unexpected repository call: " + m.Name);
});
var templates = Stub<ITemplateRepository>((m, a) => Task.FromResult<TemplateResponse?>(template));
var contacts = new List<ContatoMassaDto> { new() { Nome = "Maria", Telefone = "5585999999999" } };
var imports = Stub<IImportacaoRepository>((m, a) => m.Name switch {
    "ObterGrupoAsync" => Task.FromResult<GrupoImportacaoResponse?>(new GrupoImportacaoResponse()),
    "ListarContatosValidosParaEnvioAsync" => Task.FromResult(contacts),
    "AtualizarEnvioGrupoIdAsync" => Task.CompletedTask,
    _ => throw new Exception(m.Name)
});
var jobs = Stub<IEnvioMassaJobManager>((m, a) => null);
var config = Stub<IConfiguracaoRepository>((m, a) => throw new Exception(m.Name));
var atendimento = Stub<IAtendimentoService>((m, a) => Task.CompletedTask);
var dispatch = new EnvioService(repository, templates, config, service, jobs, imports, atendimento, Options.Create(new DefaultsOptions()));
handler.Status = HttpStatusCode.Created;
foreach (var mode in new[] { "template", "replacement", "removed" }) {
    var requested = mode == "replacement" ? replacement : null;
    var useTemplate = mode != "removed";
    var expected = mode == "template" ? image : requested;
    foreach (var flow in new[] { "unitario", "massa", "grupo" }) {
        saved = null;
        if (flow == "unitario")
            await dispatch.EnviarUnitarioAsync(1, new() { TemplateId = 1, Nome = "Maria", Telefone = "5585999999999", Imagem = requested, UsarImagemTemplate = useTemplate });
        else if (flow == "massa")
            await dispatch.CriarEIniciarEnvioMassaAsync(1, new() { TemplateId = 1, Contatos = contacts, IntervaloMs = 1000, Imagem = requested, UsarImagemTemplate = useTemplate });
        else
            await dispatch.CriarEIniciarEnvioMassaPorGrupoAsync(1, 1, "admin", new() { TemplateId = 1, IntervaloMs = 1000, Imagem = requested, UsarImagemTemplate = useTemplate });
        Check(ReferenceEquals(saved, expected), flow + ": " + mode);
        Check(ReferenceEquals(template.Imagem, image), "Template original deve permanecer intacto");
    }
}
Console.WriteLine("OK: imagem do template, substituição e remoção nos três fluxos de envio.");

Console.WriteLine("OK: validação de imagem, payload de mídia, legenda, envio de texto e erro da Evolution.");

class CaptureHandler : HttpMessageHandler {
    public string Path = "", Body = "";
    public HttpStatusCode Status = HttpStatusCode.Created;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        Path = request.RequestUri!.AbsolutePath;
        Body = await request.Content!.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(Status) { Content = new StringContent("{\"key\":{\"id\":\"abc\"}}", Encoding.UTF8, "application/json") };
    }
}

public class TestProxy : DispatchProxy {
    public Func<MethodInfo, object?[], object?> Call = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Call(method!, args ?? []);
}
