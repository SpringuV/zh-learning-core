using System.Text.Json;
namespace Interface;

public interface IOcrClient
{
    Task<JsonElement> PostFileAsync(string route, IFormFile file, CancellationToken cancellationToken);
    Task<JsonElement> GetAsync(string route, CancellationToken cancellationToken);
}
