using System.Security.Claims;

namespace Interface;

public interface IMediaUploadSigner
{
    Task<SignUploadUrlResponse> CreateSignedUploadUrlAsync(
        SignUploadUrlRequest request, // DTO chứa thông tin cần thiết để tạo signed URL
        ClaimsPrincipal user, // thông tin người dùng để kiểm tra quyền hạn
        CancellationToken cancellationToken = default);
}
