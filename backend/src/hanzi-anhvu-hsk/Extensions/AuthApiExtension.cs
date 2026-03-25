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

        group.MapPost("/register", () => { });
        group.MapPost("/refresh-token", AuthApi.RefreshToken);
        group.MapPost("/change-email", () => { });
        group.MapPost("/change-password", () => { });
        return group;
    }
}
