namespace Model;

public sealed class MediaUploadOptions
{
    public const string SectionName = "MediaUpload";

    public string Endpoint { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    public string AccessKeyId { get; init; } = string.Empty;
    public string SecretAccessKey { get; init; } = string.Empty;
    public string PublicBaseUrl { get; init; } = string.Empty;
    public int SignedUrlExpirySeconds { get; init; } = 900;

    // Manual kill switch: false => block all new signed upload URL requests.
    public bool UploadsEnabled { get; init; } = true;
    public string StopReason { get; init; } = "Uploads are temporarily disabled by administrator.";

    // Optional usage guard: if enabled and current usage reaches threshold, block new uploads.
    public bool EnableUsageHardStop { get; init; }
    public decimal CurrentUsagePercent { get; init; }
    public decimal HardStopUsagePercent { get; init; } = 95m; // 95m means 95%

    // Cloudflare R2 metrics sync (GET /accounts/{account_id}/r2/metrics)
    public bool EnableCloudflareUsageSync { get; init; }
    public string CloudflareAccountId { get; init; } = string.Empty;
    public string CloudflareApiToken { get; init; } = string.Empty;
    public int UsageSyncIntervalSeconds { get; init; } = 300;
    public long StorageBudgetBytes { get; init; } = 10L * 1024 * 1024 * 1024;
}
