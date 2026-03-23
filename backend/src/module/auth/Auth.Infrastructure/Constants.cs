namespace Auth.Application
{
    public class Constants
    {
        // limitter
        public const string CONCURRENCY_LIMIT = "Concurrency";
        public const string BUCKET_TOKEN_LIMIT = "TokenBucket";
        public const string FIXED_WINDOW_LIMIT = "FixedWindow";
        public const string SLIDING_WINDOW_LIMIT = "SlidingWindow";

        // auth
        public const string JWT_ISSUER = "spring-uv-19";
        public const string ADMINISTRATORS = "Administrators";
        public const string USERS = "Users";
        public const string POLICY_USER_ONLY = "UserOnly";
        public const string POLICY_ADMIN_ONLY = "AdminOnly";
        public const string POLICY_ADMIN_OR_USER = "AdminOrUser";
        // TODO: Don't use this in production
        public const string DEFAULT_PASSWORD = "Admin@123";
        // TODO: Change this to an environment variable
        public const string JWT_SECRET_KEY = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytesSpring1907";
    }
}
