namespace Interface;

public interface IMediaUsageState
{
    decimal? CurrentUsagePercent { get; }
    long? CurrentUsageBytes { get; }
    DateTimeOffset? LastUpdatedUtc { get; }

    void Update(decimal usagePercent, long usageBytes, DateTimeOffset updatedAtUtc);
}
