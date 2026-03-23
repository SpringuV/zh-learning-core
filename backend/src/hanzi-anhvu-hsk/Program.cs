using System.Text.Json;
using HanziAnhvuHsk.Services;
using Interface;
using Model;

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
app.Run();
