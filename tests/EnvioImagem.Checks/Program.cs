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
