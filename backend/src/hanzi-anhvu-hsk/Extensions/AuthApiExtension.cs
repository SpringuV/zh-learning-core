using HanziAnhVuHsk.Api.Apis;
using Auth.Contracts.DTOs;

namespace HanziAnhVuHsk.Api.Extensions;

public static class AuthApiExtension
{
    public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/auth/v{version:int}").MapAuthApi().WithTags("Auth Service Api");
        return builder;
    }

    public static RouteGroupBuilder MapAuthApi(this RouteGroupBuilder group)
    {
        group.MapPost("/login", AuthApi.Login)
            .Accepts<LoginRequest>("application/json");
        // .Accepts<LoginRequest>("application/json") // support for swagger
        //.RequireAuthorization("AuthenticatedUser");

        group.MapPost("/register", AuthApi.Register);
        group.MapPost("/activate", AuthApi.ActiveAccount);
        group.MapPost("/refresh-token", AuthApi.RefreshToken);
        group.MapPost("/change-email", () => { });
        group.MapPost("/change-password", () => AuthApi.ChangePassword);
        group.MapPost("/logout", AuthApi.Logout);
        group.MapPost("/forgot-password", () => { });
        group.MapPost("/reset-password", () => { });
        group.MapPost("/verify-email", () => { });
        group.MapPost("/activate/resend", AuthApi.ResendLinkActivation);
        return group;
    }
}
