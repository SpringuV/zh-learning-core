namespace HanziAnhVuHsk.Extensions;
public static class LessonApiExtensions
{
    public static IEndpointRouteBuilder MapLessonApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/lesson/v{version:int}")
            .MapLessonApi()
            .WithTags("Lesson Api");
        return builder;
    }
    public static RouteGroupBuilder MapLessonApi(this RouteGroupBuilder group)
    {
        group.MapPost("/course", LessonApi.CreateCourse).WithName("CreateCourse");
        group.MapPost("/topic", LessonApi.CreateTopic).WithName("CreateTopic");
        return group;
    }
}
