using System.Security.Claims;

namespace Interface;

public interface IMediaUploadSigner
{
    Task<SignUploadUrlResponse> CreateSignedUploadUrlAsync(
        SignUploadUrlRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}
