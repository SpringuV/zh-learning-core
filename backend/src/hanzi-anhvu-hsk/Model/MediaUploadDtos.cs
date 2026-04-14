namespace Model;

public sealed record SignUploadUrlRequest(
    string FileName,
    string ContentType,
    string Folder,
    int? ExpirySeconds = null
);

public sealed record SignUploadUrlResponse(
    string UploadUrl,
    string HttpMethod,
    string ObjectKey,
    string PublicUrl,
    DateTime ExpiresAtUtc
);
