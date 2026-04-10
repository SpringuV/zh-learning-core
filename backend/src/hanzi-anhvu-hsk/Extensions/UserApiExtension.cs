namespace HanziAnhVuHsk.Api.Extensions;

public static class UserApiExtension
{
    public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/user/v{version:int}")
            .MapUserApi()
            .WithTags("User Api");
        return builder;
    }
    public static RouteGroupBuilder MapUserApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", UserSearchApi.SearchUsers);

        return group;
    }
}
