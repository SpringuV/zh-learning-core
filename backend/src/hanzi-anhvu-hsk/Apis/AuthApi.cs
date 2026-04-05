using Auth.Contracts;
using Auth.Contracts.DTOs;
using Auth.Domain.Exceptions;
using HanziAnhVuHsk.Api.Config;
using Microsoft.AspNetCore.Mvc;

namespace HanziAnhVuHsk.Api.Apis;

public class AuthApi
{
    public static async Task<IResult> UpdateProfile([FromBody] UpdateProfileRequest request, HttpContext httpContext, IAuthService authService, CancellationToken ct)
    {
        try
        {
            // lấy userId từ token (đã được middleware xác thực và gắn vào HttpContext.User)
        var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub");
        if (userIdClaim is null) { 
            return Results.BadRequest(new { Message = "User ID not found in token." });
        }
        var result = await authService.UpdateProfileAsync(Guid.Parse(userIdClaim.Value), request, ct);
        return result ? Results.Ok(new { Message = "Cập nhật hồ sơ thành công." }) : Results.BadRequest(new { Message = "Cập nhật hồ sơ thất bại." });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }
    }
    public static async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest request, HttpContext httpContext, IAuthService authService, CancellationToken ct)
    {
        try
        {
            // lấy userId từ token (đã được middleware xác thực và gắn vào HttpContext.User)
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub");
            if (userIdClaim is null) { 
                return Results.BadRequest(new { Message = "User ID not found in token." });
            }
            var userId = Guid.Parse(userIdClaim.Value);
            var result = await authService.ChangePasswordAsync(userId, request, ct);
            return result ? Results.Ok(new { Message = "Đổi mật khẩu thành công." }) : Results.BadRequest(new { Message = "Mật khẩu cũ không đúng." });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }
    }

    public static async Task<IResult> ActiveAccount([FromBody] ActivateAccountRequest request, IAuthService authService, CancellationToken ct)
    {
        try
        {
            var result = await authService.ActivateAccountAsync(request, ct);
            return result ? Results.Ok(new { Message = "Kích hoạt tài khoản thành công." }) : Results.BadRequest(new { Message = "Mã kích hoạt không hợp lệ hoặc đã hết hạn." });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }
    }

    public static async Task<IResult> ResendLinkActivation([FromBody] ResendActivationRequest request, HttpContext context, IAuthService authService, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.ResendActivateAccountAsync(request, cancellationToken);
            return result 
                ? Results.Ok(new { Message = "Đã gửi lại email kích hoạt. Vui lòng kiểm tra hộp thư của bạn." }) 
                : Results.BadRequest(new { Message = "Email không tồn tại hoặc tài khoản đã được kích hoạt." });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Results.StatusCode(499); // Client Closed Request
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }
    }

    //public static async Task<IResult> VerifyEmail()
    public static async Task<IResult> Logout(HttpContext httpContext, IAuthService authService, CancellationToken ct)
    {
        string refreshToken = httpContext.Request.Cookies[ConfigureCookieSettings.RefreshTokenCookieName] ?? string.Empty;
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.BadRequest(new { Message = "Refresh token bị thiếu." });
        }
        try
        {
            var result = await authService.LogoutAsync(refreshToken, ct);
            // Xóa cookie sau khi logout
            httpContext.Response.Cookies.Delete(ConfigureCookieSettings.IdentifierCookieName);
            httpContext.Response.Cookies.Delete(ConfigureCookieSettings.RefreshTokenCookieName, new CookieOptions
            {
                Path = "/" // Phải chỉ định path giống với lúc tạo cookie để xóa đúng cookie đó
            });
            return Results.Ok(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }
    }

    public static async Task<IResult> Register([FromBody] RegisterRequest request, HttpContext httpContext, IAuthService authService, CancellationToken ct)
    { 
        try
        {
            var result = await authService.RegisterAsync(request, ct);
            return Results.Ok(new { result.Message, result.UserId });
        } catch(OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }

    }

    public static async Task<IResult> Login([FromBody] LoginRequest request, HttpContext httpContext, IAuthService authService, CancellationToken ct)
    {
        try
        {
            var result = await authService.LoginAsync(request, ct);

            SetTokenCookies(
                httpContext.Response,
                result.AccessToken,
                result.RefreshToken,
                result.AccessTokenExpiresAt,
                result.RefreshTokenExpiresAt);
            return Results.Ok(new {UserId = result.UserId, UserName = result.UserName, Roles = result.Roles, Message = result.Message});
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { Message = ex.Message });
        }
    }

    public static async Task<IResult> RefreshToken(HttpContext httpContext, IAuthService authService, CancellationToken ct)
    {
        string refreshToken = httpContext.Request.Cookies[ConfigureCookieSettings.RefreshTokenCookieName] ?? string.Empty;
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.BadRequest(new { Message = "Refresh token bị thiếu." });
        }
        try
        {
            var result = await authService.RefreshTokenAsync(refreshToken, ct);

            SetTokenCookies(
                httpContext.Response,
                result.AccessToken,
                result.RefreshToken,
                result.AccessTokenExpiresAt,
                result.RefreshTokenExpiresAt);
            return Results.Ok(new {result.Message});
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }
    }

    private static void SetTokenCookies(
        HttpResponse httpResponse,
        string accessToken,
        string refreshToken,
        DateTimeOffset accessTokenExpiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        // Keep cookie policy consistent for both access and refresh tokens.
        // If current request is HTTPS, use Secure + SameSite=None so cookies are sent in cross-site XHR.
        bool isSecure = httpResponse.HttpContext.Request.IsHttps;
        var sameSiteMode = isSecure ? SameSiteMode.None : SameSiteMode.Lax;
        
        httpResponse.Cookies.Append(ConfigureCookieSettings.IdentifierCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = sameSiteMode,
            Expires = accessTokenExpiresAt,
            Path = "/"
        });

        // Refresh token: dài hạn (7 ngày), gửi cho tất cả endpoints trên domain
        // Path="/" để cookie được gửi với mọi request, không chỉ /api/auth
        httpResponse.Cookies.Append(ConfigureCookieSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = sameSiteMode,
            Expires = refreshTokenExpiresAt,
            Path = "/"
        });
    }
}
