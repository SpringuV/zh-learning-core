namespace Search.Contracts;

public static class SearchCacheScopes
{
    public const string CourseAdminSearch = "search:course:admin";
    public const string TopicAdminSearch = "search:topic:admin";
    public const string ExerciseAdminSearch = "search:exercise:admin";
    public const string UserAdminSearch = "search:user:admin";
    public const string CoursePublicSearch = "search:course:public";
    public const string TopicPublicSearch = "search:topic:public";
    public const string ExercisePublicSearch = "search:exercise:public";
    public const string ExerciseSessionItemsSnapshot = "search:session-items-snapshot";
    public const string TopicExerciseSessionSearch = "search:topic-exercise-session";
    public const string ExerciseSessionPracticeItemWithoutAnswer = "search:session-practice-item-without-answer";
}