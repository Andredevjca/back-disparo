using System.Text.Json.Serialization;
using DisparoApi.Models;

namespace DisparoApi.Dtos;

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class MinhaContaResponse
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class UsuarioCreateDto
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class UsuarioUpdateDto
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

public class UsuarioResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class TemplateCreateUpdateDto
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;
}

public class TemplateResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class ConfiguracaoResponse
{
    [JsonPropertyName("intervaloMs")]
    public int IntervaloMs { get; set; }
}

public class ConfiguracaoUpdateDto
{
    [JsonPropertyName("intervaloMs")]
    public int IntervaloMs { get; set; }
}

public class ContaWhatsAppCreateDto
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;
}

public class EnvioUnitarioRequest
{
    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("templateId")]
    public int? TemplateId { get; set; }

    [JsonPropertyName("templateNome")]
    public string? TemplateNome { get; set; }

    [JsonPropertyName("instance")]
    public string Instance { get; set; } = string.Empty;
}

public class EnvioUnitarioResponse
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("erro")]
    public string? Erro { get; set; }

    [JsonPropertyName("evolucaoId")]
    public string? EvolucaoId { get; set; }

    [JsonPropertyName("evolution_id")]
    public string? EvolutionId { get; set; }
}

public class ContatoMassaDto
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("dados")]
    public Dictionary<string, string>? Dados { get; set; }
}

public class EnvioMassaRequest
{
    [JsonPropertyName("contatos")]
    public List<ContatoMassaDto> Contatos { get; set; } = new();

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("intervaloMs")]
    public int? IntervaloMs { get; set; }

    [JsonPropertyName("templateId")]
    public int? TemplateId { get; set; }

    [JsonPropertyName("templateNome")]
    public string? TemplateNome { get; set; }

    [JsonPropertyName("instance")]
    public string Instance { get; set; } = string.Empty;
}

public class EnvioMassaResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class EnvioResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonPropertyName("template_id")]
    public int? TemplateId { get; set; }

    [JsonPropertyName("template_nome")]
    public string? TemplateNome { get; set; }

    [JsonPropertyName("intervalo_ms")]
    public int? IntervaloMs { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("enviados")]
    public int Enviados { get; set; }

    [JsonPropertyName("erros")]
    public int Erros { get; set; }

    [JsonPropertyName("pendentes")]
    public int Pendentes { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("instancia")]
    public string? Instancia { get; set; }

    [JsonPropertyName("numero_origem")]
    public string? NumeroOrigem { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("finished_at")]
    public DateTime? FinishedAt { get; set; }
}

public class EnvioDetalheResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("envio_id")]
    public int EnvioId { get; set; }

    [JsonPropertyName("grupo_importacao_id")]
    public int? GrupoImportacaoId { get; set; }

    [JsonPropertyName("usuario_id")]
    public int? UsuarioId { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("mensagem")]
    public string? Mensagem { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("erro")]
    public string? Erro { get; set; }

    [JsonPropertyName("evolution_id")]
    public string? EvolutionId { get; set; }

    [JsonPropertyName("numero_origem")]
    public string? NumeroOrigem { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("enviado_em")]
    public DateTime? EnviadoEm { get; set; }

    [JsonPropertyName("tipo")]
    public string? Tipo { get; set; }

    [JsonPropertyName("template_nome")]
    public string? TemplateNome { get; set; }

    [JsonPropertyName("instancia")]
    public string? Instancia { get; set; }

    [JsonPropertyName("envio_origem")]
    public string? EnvioOrigem { get; set; }
}

public class EnvioMassaGetResponse
{
    [JsonPropertyName("envio")]
    public EnvioResponse Envio { get; set; } = new();

    [JsonPropertyName("detalhes")]
    public List<EnvioDetalheResponse> Detalhes { get; set; } = new();
}

public class HistoricoFiltroDto
{
    public int? Page { get; set; }
    public int? PerPage { get; set; }
    public string? Status { get; set; }
    public string? Telefone { get; set; }
    public string? Nome { get; set; }
    public string? Instancia { get; set; }
    public int? GrupoImportacaoId { get; set; }
    public string? De { get; set; }
    public string? Ate { get; set; }
}

public class HistoricoPaginadoResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("rows")]
    public List<EnvioDetalheResponse> Rows { get; set; } = new();
}

public class ErrorMessageResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class GrupoImportacaoResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("usuario_id")]
    public int UsuarioId { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("arquivo_nome")]
    public string? ArquivoNome { get; set; }

    [JsonPropertyName("total_linhas")]
    public int TotalLinhas { get; set; }

    [JsonPropertyName("validos")]
    public int Validos { get; set; }

    [JsonPropertyName("invalidos")]
    public int Invalidos { get; set; }

    [JsonPropertyName("duplicados")]
    public int Duplicados { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class ContatoImportadoResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("grupo_id")]
    public int GrupoId { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("telefone_normalizado")]
    public string? TelefoneNormalizado { get; set; }

    [JsonPropertyName("telefone_original")]
    public string? TelefoneOriginal { get; set; }

    [JsonPropertyName("dados")]
    public Dictionary<string, object?>? Dados { get; set; }

    [JsonPropertyName("status_validacao")]
    public string StatusValidacao { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class ImportarPlanilhaResponse
{
    [JsonPropertyName("grupo_id")]
    public int GrupoId { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("validos")]
    public int Validos { get; set; }

    [JsonPropertyName("invalidos")]
    public int Invalidos { get; set; }

    [JsonPropertyName("duplicados")]
    public int Duplicados { get; set; }
}

public class GrupoDetalhePaginadoResponse
{
    [JsonPropertyName("grupo")]
    public GrupoImportacaoResponse Grupo { get; set; } = new();

    [JsonPropertyName("contatos")]
    public List<ContatoImportadoResponse> Contatos { get; set; } = new();

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class EnvioPorGrupoRequest
{
    [JsonPropertyName("templateId")]
    public int? TemplateId { get; set; }

    [JsonPropertyName("mensagem")]
    public string? Mensagem { get; set; }

    [JsonPropertyName("templateNome")]
    public string? TemplateNome { get; set; }

    [JsonPropertyName("intervaloMs")]
    public int? IntervaloMs { get; set; }

    [JsonPropertyName("instance")]
    public string Instance { get; set; } = string.Empty;
}
