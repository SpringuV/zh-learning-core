namespace HanziAnhvuHsk.Services;

public sealed class InMemoryMediaUsageState : IMediaUsageState
{
    private readonly object _lock = new();
    private decimal? _currentUsagePercent;
    private long? _currentUsageBytes;
    private DateTimeOffset? _lastUpdatedUtc;

    public decimal? CurrentUsagePercent
    {
        get
        {
            lock (_lock) // Đảm bảo thread-safety khi đọc giá trị
            {
                return _currentUsagePercent;
            }
        }
    }

    public long? CurrentUsageBytes
    {
        get
        {
            lock (_lock) // Đảm bảo thread-safety khi đọc giá trị
            {
                return _currentUsageBytes;
            }
        }
    }

    public DateTimeOffset? LastUpdatedUtc
    {
        get
        {
            lock (_lock) // Đảm bảo thread-safety khi đọc giá trị
            {
                return _lastUpdatedUtc;
            }
        }
    }

    public void Update(decimal usagePercent, long usageBytes, DateTimeOffset updatedAtUtc)
    {
        lock (_lock) // Đảm bảo thread-safety khi cập nhật giá trị
        {
            _currentUsagePercent = usagePercent;
            _currentUsageBytes = usageBytes;
            _lastUpdatedUtc = updatedAtUtc;
        }
    }
}
