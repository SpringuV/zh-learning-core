namespace HanziAnhVuHsk.Extensions;
public static class ClassroomApiExtensions
{
    public static IEndpointRouteBuilder MapClassroomApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/classroom/v{version:int}")
            .MapClassroomApi()
            .WithTags("Classroom Api");
        return builder;
    }
    public static RouteGroupBuilder MapClassroomApi(this RouteGroupBuilder group)
    {
        // group.MapGet("/", ClassroomSearchApi.SearchClassrooms);

        return group;
    }
}
