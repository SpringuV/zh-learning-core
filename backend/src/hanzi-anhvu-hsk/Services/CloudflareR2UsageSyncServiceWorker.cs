using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HanziAnhvuHsk.Services;

public sealed class CloudflareR2UsageSyncService(
    IOptionsMonitor<MediaUploadOptions> optionsMonitor, // IOptionsMonitor để theo dõi thay đổi cấu hình media upload, cho phép bật/tắt tính năng sync usage và điều chỉnh tần suất sync mà không cần khởi động lại ứng dụng, cách dùng sẽ được thể hiện trong phần ExecuteAsync của BackgroundService.
    IHttpClientFactory httpClientFactory, // IHttpClientFactory để tạo HttpClient có cấu hình sẵn để gọi API của Cloudflare, giúp quản lý kết nối hiệu quả và tái sử dụng HttpClient, gọi là webhook để lấy thông tin usage metrics của R2 từ Cloudflare.
    IMediaUsageState mediaUsageState, // IMediaUsageState để lưu trữ trạng thái usage hiện tại của media, bao gồm phần trăm đã sử dụng và số bytes đã dùng, thông tin này sẽ được cập nhật sau khi gọi API của Cloudflare và có thể được các phần khác của ứng dụng truy xuất để hiển thị hoặc đưa ra cảnh báo khi gần đạt đến giới hạn lưu trữ.
    ILogger<CloudflareR2UsageSyncService> logger) : BackgroundService
{
    // Định nghĩa các storage classes mà R2 có thể sử dụng để lưu trữ dữ liệu, bao gồm "standard" và "infrequentAccess". Các storage class này sẽ được sử dụng khi truy xuất dữ liệu từ API của Cloudflare để tính toán tổng dung lượng đã sử dụng.
    private static readonly string[] StorageClasses = ["standard", "infrequentAccess"];

    private readonly IOptionsMonitor<MediaUploadOptions> _optionsMonitor = optionsMonitor;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMediaUsageState _mediaUsageState = mediaUsageState;
    private readonly ILogger<CloudflareR2UsageSyncService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Lấy cấu hình hiện tại từ IOptionsMonitor, bao gồm việc kiểm tra xem tính năng sync usage có được bật hay không và tần suất sync được đặt là bao nhiêu. Sau đó, nếu tính năng sync được bật, gọi phương thức SyncUsageOnceAsync để thực hiện việc đồng bộ usage một lần. Nếu có lỗi xảy ra trong quá trình đồng bộ, sẽ được bắt và ghi log cảnh báo. Cuối cùng, chờ một khoảng thời gian được định nghĩa bởi tần suất sync trước khi thực hiện vòng lặp tiếp theo, cho phép việc điều chỉnh tần suất sync thông qua cấu hình mà không cần khởi động lại ứng dụng.
            var options = _optionsMonitor.CurrentValue;
            // Tính toán khoảng thời gian delay giữa các lần sync dựa trên cấu hình UsageSyncIntervalSeconds, đảm bảo rằng giá trị này nằm trong khoảng từ 30 giây đến 3600 giây (1 giờ) để tránh việc sync quá thường xuyên hoặc quá hiếm.
            var interval = TimeSpan.FromSeconds(Math.Clamp(options.UsageSyncIntervalSeconds, 30, 3600));

            try
            {
                if (options.EnableCloudflareUsageSync)
                {
                    await SyncUsageOnceAsync(options, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error while syncing Cloudflare R2 usage metrics.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task SyncUsageOnceAsync(MediaUploadOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CloudflareAccountId) ||
            string.IsNullOrWhiteSpace(options.CloudflareApiToken) ||
            options.StorageBudgetBytes <= 0)
        {
            _logger.LogWarning(
                "Cloudflare usage sync is enabled but configuration is incomplete. Please set CloudflareAccountId, CloudflareApiToken, and positive StorageBudgetBytes.");
            return;
        }

        var client = _httpClientFactory.CreateClient("CloudflareApi");
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"accounts/{options.CloudflareAccountId}/r2/metrics");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.CloudflareApiToken);

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Cloudflare usage sync failed with status {StatusCode}. Response: {ResponseBody}",
                (int)response.StatusCode,
                Truncate(body, 400));
            return;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;
        if (!TryGetBoolean(root, "success", out var success) || !success)
        {
            _logger.LogWarning("Cloudflare usage sync returned success=false.");
            return;
        }

        if (!TryGetPropertyCaseInsensitive(root, "result", out var result))
        {
            _logger.LogWarning("Cloudflare usage sync response does not contain result payload.");
            return;
        }

        var publishedBytes = GetStorageBytes(result, "published");
        var uploadedBytes = GetStorageBytes(result, "uploaded");
        var usedBytes = publishedBytes > 0 ? publishedBytes : uploadedBytes;

        if (usedBytes < 0)
        {
            _logger.LogWarning("Cloudflare usage sync received invalid bytes value: {UsedBytes}", usedBytes);
            return;
        }

        var usagePercent = (decimal)usedBytes * 100m / options.StorageBudgetBytes;
        var previousPercent = _mediaUsageState.CurrentUsagePercent;

        _mediaUsageState.Update(
            usagePercent: usagePercent,
            usageBytes: usedBytes,
            updatedAtUtc: DateTimeOffset.UtcNow);

        if (!previousPercent.HasValue || Math.Abs(usagePercent - previousPercent.Value) >= 1m)
        {
            _logger.LogInformation(
                "Cloudflare R2 usage synced: {UsedBytes} bytes ({UsagePercent:0.##}% of budget).",
                usedBytes,
                usagePercent);
        }
    }

    private static long GetStorageBytes(JsonElement result, string stateName)
    {
        long total = 0;

        foreach (var storageClass in StorageClasses)
        {
            total += GetLongByPath(result, storageClass, stateName, "payloadSize");
            total += GetLongByPath(result, storageClass, stateName, "metadataSize");
        }

        return total;
    }

    private static long GetLongByPath(JsonElement element, params string[] path)
    {
        var current = element;

        foreach (var part in path)
        {
            if (!TryGetPropertyCaseInsensitive(current, part, out current))
            {
                return 0;
            }
        }

        if (current.ValueKind == JsonValueKind.Number && current.TryGetInt64(out var value))
        {
            return value;
        }

        return 0;
    }

    private static bool TryGetBoolean(JsonElement element, string name, out bool value)
    {
        value = false;

        if (!TryGetPropertyCaseInsensitive(element, name, out var property))
        {
            return false;
        }

        if (property.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            value = property.GetBoolean();
            return true;
        }

        return false;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string name, out JsonElement value)
    {
        value = default;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (element.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        return false;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}
