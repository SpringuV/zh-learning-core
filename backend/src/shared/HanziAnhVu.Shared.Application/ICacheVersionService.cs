namespace HanziAnhVu.Shared.Application;

public interface ICacheVersionService
{
    Task<string> GetVersionTokenAsync(string scope, CancellationToken ct = default);

    Task InvalidateScopeAsync(string scope, CancellationToken ct = default);
}