using AspireHanziAnhVu.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var elasticPassword = builder.AddParameter("elastic-password", "elastic-test", secret: true);

var redis = builder.AddRedis("redis-hanzi");
var postgres = builder.AddPostgres("postgres")
    .WithPgWeb()
    .WithImage("postgres:17-alpine")
    .WithEnvironment("POSTGRES_MAX_PREPARED_TRANSACTIONS", "100") 
    .WithDataVolume();

var authDb = postgres.AddDatabase("identity-hanzi");
var usersDb = postgres.AddDatabase("users-hanzi");
var lessonDb = postgres.AddDatabase("lesson-hanzi");
var classroomDb = postgres.AddDatabase("classrooms-hanzi");
var elasticsearch = builder
    .AddElasticsearchWithKibana("elastic-hanzi", elasticPassword)
    .WithElasticsearchSetup();

// add project api vào
builder.AddProject<Projects.HanziAnhVuHsk_Api>("hanzi-anhvu-hsk-api")
    .WithReference(redis) // add redis
    .WithReference(authDb) // add db auth/identity
    .WithReference(usersDb) // add db users
    .WithReference(lessonDb) // add db lesson
    .WithReference(classroomDb) // add db classroom
    .WithReference(elasticsearch)

    .WithEnvironment("ConnectionStrings__AuthIdentityDbConnection", authDb)
    .WithEnvironment("ConnectionStrings__UsersDbConnection", usersDb)
    .WithEnvironment("ConnectionStrings__LessonDbConnection", lessonDb)
    .WithEnvironment("ConnectionStrings__ClassroomDbConnection", classroomDb)
    .WithEnvironment("ASPNETCORE_URLS", "https://localhost:1907;http://localhost:1908")
    .WaitFor(authDb)
    .WaitFor(lessonDb)
    .WaitFor(usersDb)
    .WaitFor(classroomDb);

builder.Build().Run();
