
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
        group.MapGet("/courses", LessonSearchApi.SearchCourses).RequireAuthorization();
        group.MapGet("/courses/dashboard", LessonSearchApi.LoadCoursesForDashboardClient).RequireAuthorization();
        group.MapGet("/topics", LessonSearchApi.SearchTopics).RequireAuthorization();
        group.MapGet("/topics/dashboard/{slug}", LessonSearchApi.SearchTopicsForClient).RequireAuthorization();
        group.MapGet("/course-topics", LessonSearchApi.SearchCourseTopicsOverview).RequireAuthorization();
        group.MapGet("/exercises", LessonSearchApi.SearchExercises).RequireAuthorization();
        group.MapGet("/topic-exercises", LessonSearchApi.SearchTopicExercisesOverview).RequireAuthorization();
        group.MapGet("/topics/{topicId:guid}", LessonSearchApi.SearchTopicDetail).RequireAuthorization();
        group.MapGet("/exercises/{exerciseId:guid}", LessonSearchApi.SearchExerciseDetail).RequireAuthorization();
        return group;
    }
    public static RouteGroupBuilder MapSearchUserApi(this RouteGroupBuilder group)
    {
        group.MapGet("/users", UserSearchApi.SearchUsers).RequireAuthorization();

        return group;
    }

}
