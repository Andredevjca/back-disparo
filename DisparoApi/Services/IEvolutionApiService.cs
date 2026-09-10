using DisparoApi.Models;

namespace DisparoApi.Services;

public interface IEvolutionApiService
{
    Task<List<WhatsAppAccount>> ListarContasAsync();
    Task<WhatsAppAccount> GarantirInstanciaEConectarAsync(string instance);
    Task<WhatsAppAccount> ConectarAsync(string instance);
    Task DesconectarAsync(string instance);
    Task RemoverInstanciaAsync(string instance);
    Task<(bool ok, string? evolutionId, string? numeroOrigem, string? erro)> EnviarMensagemAsync(string instance, string telefone, string mensagem);
}
