using HanziAnhVuHsk.Apis.LessonApi;

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
        // topic progress + exercise session
        group.MapPost("/topic-progress/exercise-session/started", TopicApi.StartLearning).WithName("HandleExerciseSessionStarted");

        // course
        group.MapPost("/course", CourseApi.CreateCourse).WithName("CreateCourse");
        group.MapPost("/course/{courseId:guid}/publish", CourseApi.PublishCourse).WithName("PublishCourse");
        group.MapPost("/course/{courseId:guid}/unpublish", CourseApi.UnPublishCourse).WithName("UnPublishCourse");
        group.MapPost("/course/reorder", CourseApi.CourseReOrder).WithName("CourseReOrder");
        group.MapPatch("/course", CourseApi.UpdateCourse).WithName("UpdateCourse");
        group.MapDelete("/course/{courseId:guid}", CourseApi.DeleteCourse).WithName("DeleteCourse");
        // topic
        group.MapPost("/topic", TopicApi.CreateTopic).WithName("CreateTopic");
        group.MapPatch("/topic", TopicApi.UpdateTopic).WithName("UpdateTopic");
        group.MapDelete("/topic/{topicId:guid}", TopicApi.DeleteTopic).WithName("DeleteTopic");
        group.MapPost("/topic/{topicId:guid}/publish", TopicApi.PublishTopic).WithName("PublishTopic");
        group.MapPost("/topic/{topicId:guid}/unpublish", TopicApi.UnPublishTopic).WithName("UnPublishTopic");
        group.MapPost("/topic/reorder", TopicApi.TopicReOrder).WithName("TopicReOrder");
        // exercise
        group.MapPost("/exercise/reorder", ExerciseApi.ExerciseReOrder).WithName("ExerciseReOrder");
        group.MapPatch("/exercise", ExerciseApi.UpdateExercise).WithName("UpdateExercise");
        group.MapPost("/exercise/{exerciseId:guid}/publish", ExerciseApi.PublishExercise).WithName("PublishExercise");
        group.MapPost("/exercise/{exerciseId:guid}/unpublish", ExerciseApi.UnPublishExercise).WithName("UnPublishExercise");
        group.MapPost("/exercise", ExerciseApi.CreateExercise).WithName("CreateExercise");
        group.MapDelete("/exercise/{exerciseId:guid}", ExerciseApi.DeleteExercise).WithName("DeleteExercise");
        return group;
    }
}
