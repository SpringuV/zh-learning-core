namespace Model;

// Đây là class chứa các cấu hình liên quan đến việc upload media, bao gồm 
// endpoint, bucket name, access key, secret key, URL public base để truy cập media sau khi upload,
// thời gian hết hạn của signed URL, cũng như các cấu hình liên quan đến việc bật/tắt tính năng upload
// và đồng bộ usage với Cloudflare R2. Các cấu hình này sẽ được sử dụng trong quá trình tạo signed URL
// để upload media và trong BackgroundService để đồng bộ usage metrics từ Cloudflare R2.
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
