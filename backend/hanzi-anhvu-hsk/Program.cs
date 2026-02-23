// initialize the web application for the hanzi-anhvu-hsk API, which configures services, middleware, and endpoints for handling HTTP requests. 
// Receives parameters from the command line, sets up controllers for handling API requests, and configures OpenAPI for API documentation.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(); // registers the controllers in the application, enabling them to handle incoming HTTP requests and generate responses.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // register services for openApi (swagger) to generate API documentation and provide an interactive interface for testing the API endpoints.

var app = builder.Build(); // builds the web application based on the configured services and middleware, preparing it to handle incoming HTTP requests.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
