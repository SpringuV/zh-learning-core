using System.Text.Json;
using HanziAnhvuHsk.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OcrServiceOptions>(
    builder.Configuration.GetSection(OcrServiceOptions.SectionName));

builder.Services.AddHttpClient<IOcrClient, OcrClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<OcrServiceOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "hanzi-anhvu-hsk-api" }));

app.MapPost("/api/ocr/image", async (IFormFile file, IOcrClient ocrClient, CancellationToken ct) =>
{
    var result = await ocrClient.PostFileAsync("/v1/ocr/image", file, ct);
    return Results.Json(result, statusCode: StatusCodes.Status200OK);
})
.Accepts<IFormFile>("multipart/form-data")
.DisableAntiforgery();

app.MapPost("/api/ocr/pdf/jobs", async (IFormFile file, IOcrClient ocrClient, CancellationToken ct) =>
{
    var result = await ocrClient.PostFileAsync("/v1/ocr/pdf/jobs", file, ct);
    return Results.Json(result, statusCode: StatusCodes.Status200OK);
})
.Accepts<IFormFile>("multipart/form-data")
.DisableAntiforgery();

app.MapGet("/api/ocr/pdf/jobs/{jobId}", async (string jobId, IOcrClient ocrClient, CancellationToken ct) =>
{
    var result = await ocrClient.GetAsync($"/v1/ocr/pdf/jobs/{jobId}", ct);
    return Results.Json(result, statusCode: StatusCodes.Status200OK);
});

app.MapGet("/api/ocr/pdf/jobs/{jobId}/result", async (string jobId, IOcrClient ocrClient, CancellationToken ct) =>
{
    var result = await ocrClient.GetAsync($"/v1/ocr/pdf/jobs/{jobId}/result", ct);
    return Results.Json(result, statusCode: StatusCodes.Status200OK);
});

app.Run();
