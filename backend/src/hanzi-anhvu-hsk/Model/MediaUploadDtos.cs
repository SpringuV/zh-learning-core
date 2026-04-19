namespace Model;

// request DTO để tạo một signed URL để upload media, bao gồm tên file, 
// content type, folder để lưu trữ trên cloud storage, và thời gian hết hạn của signed URL (tùy chọn).
public sealed record SignUploadUrlRequest(
    string FileName,
    string ContentType,
    string Folder,
    int? ExpirySeconds = null
);

// đây là DTO để trả về thông tin cần thiết cho FE khi tạo một signed URL để upload media,
// bao gồm URL để upload, HTTP method, object key để lưu trữ trên cloud storage,
// URL public để truy cập media sau khi upload thành công, và thời gian hết hạn của signed URL.
public sealed record SignUploadUrlResponse(
    string UploadUrl,
    string HttpMethod,
    string ObjectKey,
    string PublicUrl,
    DateTime ExpiresAtUtc
);
