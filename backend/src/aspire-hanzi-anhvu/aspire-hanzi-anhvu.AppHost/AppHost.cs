var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres")
    .WithPgWeb()
    .WithImage("postgres:17-alpine")
    .WithDataVolume();

var authDb = postgres.AddDatabase("identity-hanzi");

var elasticsearch = builder.AddElasticsearch("elastic-hanzi");

// add project api vào
builder.AddProject<Projects.HanziAnhVuHsk_Api>("hanzi-anhvu-hsk-api")
    .WithReference(redis) // add redis
    .WithReference(authDb) // add db auth/identity
    .WithReference(elasticsearch)
    .WithEnvironment("ConnectionStrings__AuthIdentityDbConnection", authDb)
    .WithEnvironment("ASPNETCORE_URLS", "https://localhost:1907;http://localhost:1908")
    .WaitFor(authDb);

builder.Build().Run();
