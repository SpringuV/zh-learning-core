namespace HanziAnhvuHsk.Services;

public interface IOcrClient
{
    Task<JsonElement> PostFileAsync(string route, IFormFile file, CancellationToken cancellationToken);
    Task<JsonElement> GetAsync(string route, CancellationToken cancellationToken);
}
