
namespace HanziAnhVuHsk.Extensions;

public static class SearchApiExtensions
{
    public static IEndpointRouteBuilder MapSearchApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/search/v{version:int}")
            .MapSearchUserApi()
            .MapSearchLessonApi()
            .WithTags("Search Api");

        return builder;
    }

    public static RouteGroupBuilder MapSearchLessonApi(this RouteGroupBuilder group)
    {
        group.MapGet("/lessons", LessonSearchApi.SearchLessons).RequireAuthorization();

        return group;
    }
    public static RouteGroupBuilder MapSearchUserApi(this RouteGroupBuilder group)
    {
        group.MapGet("/users", UserSearchApi.SearchUsers).RequireAuthorization();

        return group;
    }
}
