using Auth.Contracts;
using Auth.Contracts.DTOs;
using HanziAnhVuHsk.Api.Config;
using Microsoft.AspNetCore.Mvc;

namespace HanziAnhVuHsk.Api.Apis;

public class AuthApi
{

    public static async Task<IResult> ActiveAccount([FromBody] ActivateAccountRequest request, HttpContext httpContext, IAuthService authService, CancellationToken ct)
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
    }

    public static async Task<IResult> ResendLinkActivation([FromBody] ResendActivationRequest request, HttpContext context, IAuthService authService , CancellationToken cancellationToken)
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
                Path = "/api/auth" // Phải chỉ định path giống với lúc tạo cookie để xóa đúng cookie đó
            });
            return Results.Ok(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
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
            return Results.Ok(new { result.Message });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
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
    }


    private static void SetTokenCookies(
        HttpResponse httpResponse,
        string accessToken,
        string refreshToken,
        DateTimeOffset accessTokenExpiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        httpResponse.Cookies.Append(ConfigureCookieSettings.IdentifierCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = accessTokenExpiresAt
        });

        // Refresh token: dài hạn (7 ngày), chỉ gửi đến các endpoint trong /api/auth (refresh + logout)
        // Path=/api/auth thay vì /api/auth/refresh để logout cũng nhận được cookie và revoke trong DB.
        httpResponse.Cookies.Append(ConfigureCookieSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = refreshTokenExpiresAt,
            Path = "/api/auth"
        });
    }

    
}
