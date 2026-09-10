using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using DisparoApi.Models;
using DisparoApi.Options;

namespace DisparoApi.Services;

public class EvolutionApiService : IEvolutionApiService
{
    private readonly HttpClient _httpClient;
    private readonly EvolutionOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public EvolutionApiService(HttpClient httpClient, IOptions<EvolutionOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        var baseUrl = string.IsNullOrWhiteSpace(_options.Url) ? "http://localhost:8080/" : (_options.Url.EndsWith('/') ? _options.Url : _options.Url + "/");
        _httpClient.BaseAddress = new Uri(baseUrl);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("apikey", _options.ApiKey);
        }
    }

    public async Task<List<WhatsAppAccount>> ListarContasAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return new List<WhatsAppAccount>();
        using var response = await _httpClient.GetAsync("instance/fetchInstances");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        var contas = new List<WhatsAppAccount>();
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                var conta = ParseAccount(item);
                if (conta != null)
                    contas.Add(conta);
            }
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in valueProp.EnumerateArray())
                {
                    var conta = ParseAccount(item);
                    if (conta != null)
                        contas.Add(conta);
                }
            }
            else
            {
                var conta = ParseAccount(root);
                if (conta != null)
                    contas.Add(conta);
            }
        }

        return contas;
    }

    private static WhatsAppAccount? ParseAccount(JsonElement item)
    {
        string? instance = null;
        string? instanceName = null;

        if (item.TryGetProperty("name", out var nameProp))
            instanceName = nameProp.GetString();
        if (item.TryGetProperty("instanceName", out var inProp))
            instanceName ??= inProp.GetString();

        if (item.TryGetProperty("instance", out var instProp))
        {
            if (instProp.ValueKind == JsonValueKind.Object)
            {
                if (instProp.TryGetProperty("instanceName", out var subIn))
                    instance = subIn.GetString();
                if (instProp.TryGetProperty("name", out var subName))
                    instance ??= subName.GetString();
            }
            else if (instProp.ValueKind == JsonValueKind.String)
            {
                instance = instProp.GetString();
            }
        }
        instance ??= instanceName;

        if (string.IsNullOrWhiteSpace(instance))
            return null;

        var state = EstadoConexao.Unknown;
        var connected = false;
        string? number = null;
        string? profileName = null;
        string? qrcode = null;

        if (item.TryGetProperty("connectionStatus", out var connStatusProp))
        {
            var s = connStatusProp.GetString()?.ToLowerInvariant();
            state = s switch
            {
                "open" or "connected" => EstadoConexao.Open,
                "connecting" => EstadoConexao.Connecting,
                "close" or "disconnected" => EstadoConexao.Close,
                _ => EstadoConexao.Unknown
            };
            connected = s is "open" or "connected";
        }

        if (item.TryGetProperty("status", out var statusProp))
        {
            var s = statusProp.GetString()?.ToLowerInvariant();
            state = s switch
            {
                "open" or "connected" => EstadoConexao.Open,
                "connecting" => EstadoConexao.Connecting,
                "close" or "disconnected" => EstadoConexao.Close,
                _ => state
            };
            connected = connected || s is "open" or "connected";
        }

        if (item.TryGetProperty("instance", out var instObj) && instObj.ValueKind == JsonValueKind.Object)
        {
            if (instObj.TryGetProperty("status", out var instStatus))
            {
                var s = instStatus.GetString()?.ToLowerInvariant();
                state = s switch
                {
                    "open" or "connected" => EstadoConexao.Open,
                    "connecting" => EstadoConexao.Connecting,
                    "close" or "disconnected" => EstadoConexao.Close,
                    _ => state
                };
                connected = connected || s is "open" or "connected";
            }
            if (instObj.TryGetProperty("ownerJid", out var ownerJid))
                number = ownerJid.GetString();
            if (instObj.TryGetProperty("owner", out var owner))
                number ??= owner.GetString();
            if (instObj.TryGetProperty("profileName", out var pn))
                profileName = pn.GetString();
        }

        if (item.TryGetProperty("ownerJid", out var ownerJidProp))
            number ??= ownerJidProp.GetString();
        if (item.TryGetProperty("owner", out var ownerProp))
            number ??= ownerProp.GetString();
        if (item.TryGetProperty("profileName", out var profileProp))
            profileName ??= profileProp.GetString();

        if (item.TryGetProperty("qrcode", out var qrObj) && qrObj.ValueKind == JsonValueKind.Object)
        {
            if (qrObj.TryGetProperty("base64", out var base64Prop))
                qrcode = base64Prop.GetString();
            if (qrObj.TryGetProperty("pairingCode", out var pcQr))
                qrcode ??= pcQr.GetString();
        }

        if (string.IsNullOrWhiteSpace(qrcode) && item.TryGetProperty("base64", out var base64Direct))
            qrcode = base64Direct.GetString();
        if (string.IsNullOrWhiteSpace(qrcode) && item.TryGetProperty("qrCode", out var qr))
            qrcode = qr.GetString();
        if (string.IsNullOrWhiteSpace(qrcode) && item.TryGetProperty("qrcode", out var qr2) && qr2.ValueKind == JsonValueKind.String)
            qrcode = qr2.GetString();
        if (string.IsNullOrWhiteSpace(qrcode) && item.TryGetProperty("pairingCode", out var pc))
            qrcode = pc.GetString();

        return new WhatsAppAccount
        {
            Instance = instance,
            State = state,
            Connected = connected,
            Number = number,
            ProfileName = profileName,
            Qrcode = qrcode
        };
    }

    private async Task<WhatsAppAccount> ObterContaOuFallbackAsync(string instance)
    {
        try
        {
            var todas = await ListarContasAsync();
            var conta = todas.FirstOrDefault(c => string.Equals(c.Instance, instance, StringComparison.OrdinalIgnoreCase));
            if (conta != null) return conta;
        }
        catch
        {
        }
        return new WhatsAppAccount
        {
            Instance = instance,
            State = EstadoConexao.Unknown,
            Connected = false,
            Number = null,
            ProfileName = null,
            Qrcode = null
        };
    }

    public async Task<WhatsAppAccount> GarantirInstanciaEConectarAsync(string instance)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Evolution API Key não configurada. Edite appsettings.json: Evolution > ApiKey.");
        try
        {
            var body = new
            {
                instanceName = instance,
                token = _options.ApiKey,
                qrcode = true,
                integration = "WHATSAPP-BAILEYS"
            };
            var json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync("instance/create", content);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao criar instância: {err}");
            }

            var respContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respContent);
            var root = doc.RootElement;
            var parsed = ParseAccount(root);
            if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Qrcode))
            {
                return parsed;
            }
        }
        catch (HttpRequestException)
        {
            throw;
        }
        return await ConectarAsync(instance);
    }

    public async Task<WhatsAppAccount> ConectarAsync(string instance)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Evolution API Key não configurada. Edite appsettings.json: Evolution > ApiKey.");
        using var response = await _httpClient.GetAsync($"instance/connect/{Uri.EscapeDataString(instance)}");
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao conectar: {err}");
        }
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        var parsed = ParseAccount(root);
        if (parsed != null)
        {
            parsed.Instance = instance;
            return parsed;
        }
        return await ObterContaOuFallbackAsync(instance);
    }

    public async Task DesconectarAsync(string instance)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Evolution API Key não configurada. Edite appsettings.json: Evolution > ApiKey.");
        using var response = await _httpClient.PostAsync($"instance/logout/{Uri.EscapeDataString(instance)}", null);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao desconectar: {err}");
        }
    }

    public async Task RemoverInstanciaAsync(string instance)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Evolution API Key não configurada. Edite appsettings.json: Evolution > ApiKey.");
        using var response = await _httpClient.DeleteAsync($"instance/delete/{Uri.EscapeDataString(instance)}");
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao remover instância: {err}");
        }
    }

    public async Task<(bool ok, string? evolutionId, string? numeroOrigem, string? erro)> EnviarMensagemAsync(string instance, string telefone, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return (false, null, null, "Evolution API Key não configurada. Edite appsettings.json: Evolution > ApiKey.");
        try
        {
            var body = new
            {
                number = telefone,
                text = mensagem,
                textMessage = new { text = mensagem },
                options = new { delay = 1200, presence = "composing" }
            };
            var json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync($"message/sendText/{Uri.EscapeDataString(instance)}", content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (false, null, null, $"Evolution respondeu {response.StatusCode}: {raw}");

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            string? evolutionId = null;
            string? numeroOrigem = null;

            if (root.TryGetProperty("key", out var keyProp))
            {
                if (keyProp.TryGetProperty("id", out var idProp))
                    evolutionId = idProp.GetString();
                if (keyProp.TryGetProperty("remoteJid", out var remoteProp))
                    numeroOrigem = remoteProp.GetString();
            }
            if (string.IsNullOrWhiteSpace(evolutionId) && root.TryGetProperty("messageID", out var midProp))
                evolutionId = midProp.GetString();
            if (string.IsNullOrWhiteSpace(evolutionId) && root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.Object)
            {
                if (msgProp.TryGetProperty("id", out var msgId))
                    evolutionId = msgId.GetString();
            }

            return (true, evolutionId, numeroOrigem, null);
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }
}
