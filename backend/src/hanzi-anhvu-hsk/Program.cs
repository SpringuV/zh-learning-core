using HanziAnhvuHsk.Services;
using Aspire.Elastic.Clients.Elasticsearch;
using HanziAnhVu.Shared.EventBus.Abstracts;
using HanziAnhVu.Shared.EventBus.InMemory;
using HanziAnhVuHsk.Api.Extensions;
using HanziAnhVuHsk.Extensions;
using Interface;
using Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OcrServiceOptions>(
    builder.Configuration.GetSection(OcrServiceOptions.SectionName));

// khai báo các service trong module auth để có thể inject vào api mà không cần reference đến project auth
// auth dependency
Auth.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);


// add elasticsearch service
builder.AddElasticsearchClient("elastic-hanzi");

builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

builder.Services.AddHttpClient<IOcrClient, OcrClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<OcrServiceOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// add auth api trước để có thể sử dụng cookie authentication trong api sau (nếu có)
app.MapAuthApi();
app.MapOcrApi();
app.MapSearchApi();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "hanzi-anhvu-hsk-api" }));
app.Run();
