using Auth.Application.Command.ActivateAccount;
using Auth.Application.Command.ChangePassword;
using Auth.Application.Command.Login;
using Auth.Application.Command.Logout;
using Auth.Application.Command.Refresh;
using Auth.Application.Command.Register;
using Auth.Application.Command.ResendMail;
using Auth.Contracts;
using Auth.Contracts.DTOs;
using Auth.Domain.Exceptions;
using HanziAnhVuHsk.Api.Config;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace HanziAnhVuHsk.Api.Apis;

public class AuthApi
{
    public static async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest request, HttpContext httpContext, IMediator mediator, CancellationToken ct)
    {
        try
        {
            // lấy userId từ token (đã được middleware xác thực và gắn vào HttpContext.User)
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub");
            if (userIdClaim is null) { 
                return Results.BadRequest(new { Message = "User ID not found in token." });
            }
            var userId = Guid.Parse(userIdClaim.Value);
            var result = await mediator.Send(new ChangePasswordCommand(userId, request.OldPassword, request.NewPassword), ct);
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

    public static async Task<IResult> ActiveAccount([FromBody] ActivateAccountRequest request, HttpContext httpContext, IMediator mediator, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new ActivateAccountCommand(request.Email, request.Code), ct);
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

    public static async Task<IResult> ResendLinkActivation([FromBody] ResendActivationRequest request, HttpContext context, IMediator mediator , CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ResendMailActivateCommand(request.Email, cancellationToken), cancellationToken);
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
    public static async Task<IResult> Logout(HttpContext httpContext, IMediator mediator, CancellationToken ct)
    {
        string refreshToken = httpContext.Request.Cookies[ConfigureCookieSettings.RefreshTokenCookieName] ?? string.Empty;
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.BadRequest(new { Message = "Refresh token bị thiếu." });
        }
        try
        {
            var result = await mediator.Send(new LogoutCommand(refreshToken), ct);
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

    public static async Task<IResult> Register([FromBody] RegisterRequest request, HttpContext httpContext, IMediator mediator, CancellationToken ct)
    { 
        try
        {
            var userId = await mediator.Send(new RegisterUserCommand(request.Email, request.Username, request.Password), ct);
            if (userId is null)
            {
                return Results.BadRequest(new { Message = "Đăng ký thất bại." });
            }

            return Results.Ok(new { Message = "Đăng ký thành công.", UserId = userId });
        } catch(OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex) when (ex is AuthDomainException || ex is UnauthorizedAccessException)
        {
            return Results.BadRequest(new { ex.Message });
        }

    }

    public static async Task<IResult> Login([FromBody] LoginRequest request, HttpContext httpContext, IMediator mediator, CancellationToken ct)
    {
        try
        {
            var tokenResult = await mediator.Send(new LoginUserCommand(request.Username, request.Password, request.TypeLogin), ct);
            if (tokenResult is null)
            {
                return Results.BadRequest(new { Message = "Tên đăng nhập hoặc mật khẩu không hợp lệ." });
            }

            var accessTokenExpiresAt = GetAccessTokenExpiresAt(tokenResult.AccessToken);
            var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(AuthTokenConstants.RefreshTokenExpireDays);

            SetTokenCookies(
                httpContext.Response,
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                accessTokenExpiresAt,
                refreshTokenExpiresAt);

            return Results.Ok(new
            {
                UserId = tokenResult.Users.Id,
                UserName = tokenResult.Users.Username,
                Roles = tokenResult.Users.Roles,
                Message = "Đăng nhập thành công."
            });
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

    public static async Task<IResult> RefreshToken(HttpContext httpContext, IMediator mediator, CancellationToken ct)
    {
        string refreshToken = httpContext.Request.Cookies[ConfigureCookieSettings.RefreshTokenCookieName] ?? string.Empty;
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.BadRequest(new { Message = "Refresh token bị thiếu." });
        }
        try
        {
            var tokenResult = await mediator.Send(new RefreshTokenCommand(refreshToken), ct);
            if (tokenResult is null)
            {
                return Results.BadRequest(new { Message = "Refresh token không hợp lệ." });
            }

            var accessTokenExpiresAt = GetAccessTokenExpiresAt(tokenResult.AccessToken);
            var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(AuthTokenConstants.RefreshTokenExpireDays);

            SetTokenCookies(
                httpContext.Response,
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                accessTokenExpiresAt,
                refreshTokenExpiresAt);
            return Results.Ok(new { Message = "Refresh token thành công." });
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

    private static DateTimeOffset GetAccessTokenExpiresAt(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(accessToken))
        {
            throw new InvalidOperationException("Access token is not a valid Microsoft JWT.");
        }

        var token = handler.ReadJwtToken(accessToken);
        return new DateTimeOffset(DateTime.SpecifyKind(token.ValidTo, DateTimeKind.Utc));
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
