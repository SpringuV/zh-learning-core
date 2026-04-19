using System.Security.Claims;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace HanziAnhvuHsk.Services;

// service tạo presigned URL để client có thể upload media trực tiếp lên R2, 
// tránh phải đi qua backend để giảm tải và độ trễ.
public sealed class R2MediaUploadSigner(
    IOptions<MediaUploadOptions> options,
    IMediaUsageState mediaUsageState) : IMediaUploadSigner
{
    // HashSet để định nghĩa các folder hợp lệ cho việc upload media, giúp validate request và đảm bảo rằng media được tổ chức theo cấu trúc hợp lý trên R2.
    private static readonly HashSet<string> AllowedFolders = ["images", "audio"];

    // Lưu trữ cấu hình media upload được inject từ appsettings thông qua IOptions, bao gồm thông tin endpoint, bucket name, access key, secret key, public base URL và thời gian hết hạn của signed URL. Các giá trị này sẽ được sử dụng để tạo presigned URL cho việc upload media lên R2.
    private readonly MediaUploadOptions _options = options.Value;
    private readonly IMediaUsageState _mediaUsageState = mediaUsageState;

    public Task<SignUploadUrlResponse> CreateSignedUploadUrlAsync(
        SignUploadUrlRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        EnsureUploadsAllowed();
        ValidateRequest(request);

        var folder = request.Folder.Trim().ToLowerInvariant();
        ValidateFolderContentType(folder, request.ContentType);

        var expirySeconds = request.ExpirySeconds ?? _options.SignedUrlExpirySeconds;
        expirySeconds = Math.Clamp(expirySeconds, 60, 3600);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(expirySeconds);

        var objectKey = BuildObjectKey(folder, request.FileName, user, now);

        var credentials = new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey);
        // Cấu hình AmazonS3Config để sử dụng endpoint của R2 và ForcePathStyle để đảm bảo URL được tạo ra phù hợp với R2.
        var s3Config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint, // ServiceURL để chỉ định endpoint của R2, thay vì sử dụng RegionEndpoint mặc định của AWS.
            ForcePathStyle = true // ForcePathStyle để đảm bảo rằng bucket name sẽ được đưa vào đường dẫn URL thay vì subdomain, điều này cần thiết cho R2 vì nó không hỗ trợ virtual-hosted style URL.
        };

        // Tạo một instance của AmazonS3Client với thông tin credentials và cấu hình đã chuẩn bị. Sau đó, tạo một GetPreSignedUrlRequest để định nghĩa các tham số cần thiết cho việc tạo presigned URL, bao gồm bucket name, object key, HTTP method (PUT), expiration time và content type. Cuối cùng, gọi GetPreSignedURL trên s3Client để nhận được URL có chứa signature mà client có thể sử dụng để upload file trực tiếp lên R2.
        using var s3Client = new AmazonS3Client(credentials, s3Config);
        // Tạo một GetPreSignedUrlRequest để định nghĩa các tham số cần thiết cho việc tạo presigned URL, bao gồm bucket name, object key, HTTP method (PUT), expiration time và content type.
        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT, // Sử dụng HTTP method PUT để client có thể upload file lên R2 bằng URL này.
            Expires = expiresAt,
            ContentType = request.ContentType.Trim()
        };

        // GetPreSignedURL sẽ tạo một URL có chứa signature dựa trên thông tin bucket, key, HTTP method, expiration time và content type. Client có thể sử dụng URL này để upload file trực tiếp lên R2 bằng phương thức PUT, và R2 sẽ xác thực signature để cho phép upload nếu hợp lệ.
        var uploadUrl = s3Client.GetPreSignedURL(presignRequest);
        var publicUrl = BuildPublicUrl(objectKey);

        var response = new SignUploadUrlResponse(
            UploadUrl: uploadUrl,
            HttpMethod: "PUT",
            ObjectKey: objectKey,
            PublicUrl: publicUrl,
            ExpiresAtUtc: expiresAt);

        return Task.FromResult(response);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) ||
            string.IsNullOrWhiteSpace(_options.BucketName) ||
            string.IsNullOrWhiteSpace(_options.AccessKeyId) ||
            string.IsNullOrWhiteSpace(_options.SecretAccessKey) ||
            string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            throw new InvalidOperationException(
                "Media upload is not configured. Please set MediaUpload options.");
        }
    }

    private void EnsureUploadsAllowed()
    {
        if (!_options.UploadsEnabled)
        {
            throw new MediaUploadBlockedException(
                string.IsNullOrWhiteSpace(_options.StopReason)
                    ? "Uploads are temporarily disabled by administrator."
                    : _options.StopReason);
        }

        if (!_options.EnableUsageHardStop)
        {
            return;
        }
        // Kiểm tra nếu tính năng hard stop theo usage được bật, thì so sánh phần trăm usage hiện tại với ngưỡng đã định nghĩa. Nếu usage hiện tại vượt quá hoặc bằng ngưỡng, sẽ ném một MediaUploadBlockedException để chặn việc tạo signed URL mới, nhằm tránh tình trạng vượt quá giới hạn lưu trữ đã đặt ra trên R2.
        var threshold = Math.Clamp(_options.HardStopUsagePercent, 1m, 100m);
        var currentUsagePercent = _mediaUsageState.CurrentUsagePercent ?? _options.CurrentUsagePercent;
        var current = Math.Max(0m, currentUsagePercent);

        if (current >= threshold)
        {
            throw new MediaUploadBlockedException(
                $"Uploads are temporarily stopped because usage reached {current:0.##}% (hard stop at {threshold:0.##}%).");
        }
    }

    private static void ValidateRequest(SignUploadUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException("FileName is required.", nameof(request.FileName));
        }

        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            throw new ArgumentException("ContentType is required.", nameof(request.ContentType));
        }

        if (string.IsNullOrWhiteSpace(request.Folder))
        {
            throw new ArgumentException("Folder is required.", nameof(request.Folder));
        }
    }

    private static void ValidateFolderContentType(string folder, string contentType)
    {
        if (!AllowedFolders.Contains(folder))
        {
            throw new ArgumentException("Folder must be one of: images, audio.", nameof(folder));
        }

        var ct = contentType.Trim().ToLowerInvariant();
        var isImage = ct.StartsWith("image/");
        var isAudio = ct.StartsWith("audio/");

        if (folder == "images" && !isImage)
        {
            throw new ArgumentException("ContentType must be an image/* type for images folder.", nameof(contentType));
        }

        if (folder == "audio" && !isAudio)
        {
            throw new ArgumentException("ContentType must be an audio/* type for audio folder.", nameof(contentType));
        }
    }

    private static string BuildObjectKey(string folder, string fileName, ClaimsPrincipal user, DateTime now)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = folder == "images" ? ".bin" : ".bin";
        }

        var userId = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "anonymous";
        var safeUserId = SanitizeSegment(userId);

        var key =
            $"lesson/{folder}/{now:yyyy/MM/dd}/{safeUserId}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

        return key;
    }

    private string BuildPublicUrl(string objectKey)
    {
        var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{objectKey}";
    }

    private static string SanitizeSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "anonymous" : sanitized;
    }
}
