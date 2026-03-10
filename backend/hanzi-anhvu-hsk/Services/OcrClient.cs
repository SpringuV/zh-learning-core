using System.Net.Http.Headers;
using System.Text.Json;

namespace HanziAnhvuHsk.Services;

public sealed class OcrClient(HttpClient httpClient) : IOcrClient
{
    public async Task<JsonElement> PostFileAsync(string route, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

        using var formData = new MultipartFormDataContent
        {
            { streamContent, "file", file.FileName }
        };

        using var response = await httpClient.PostAsync(route, formData, cancellationToken);
        return await ParseResponse(response, cancellationToken);
    }

    public async Task<JsonElement> GetAsync(string route, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(route, cancellationToken);
        return await ParseResponse(response, cancellationToken);
    }

    private static async Task<JsonElement> ParseResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OCR service request failed with {(int)response.StatusCode}: {payload}");
        }

        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}
