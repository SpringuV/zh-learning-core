using Aspire.Elastic.Clients.Elasticsearch;
using Auth.Infrastructure;
using HanziAnhVu.Shared.EventBus.Abstracts;
using HanziAnhVu.Shared.EventBus.InMemory;
using HanziAnhvuHsk.Services;
using HanziAnhVuHsk.Api.Config;
using HanziAnhVuHsk.Api.Extensions;
using HanziAnhVuHsk.Extensions;
using Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using Model;
using Notification.Application.Config;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://127.0.0.1:3000",
                "https://127.0.0.1:3000") // Frontend URLs
            .AllowCredentials() // Allow cookies/credentials
            .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
            .AllowAnyHeader(); // Allow all headers
    });
});

builder.Services.Configure<OcrServiceOptions>(
    builder.Configuration.GetSection(OcrServiceOptions.SectionName));

// Configure MailSettings for notification
builder.Services.Configure<MailSettings>(
    // get mail settings từ config và bind vào MailSettings class để có thể inject vào các service trong module notification
    // trình tự lấy section -> bind vào class -> inject vào service
    // services.Configure<MailSettings>(builder.Configuration.GetSection("Notification:MailSettings"))
    // cũng được nhưng sẽ bị hardcode tên section, nên tốt hơn là dùng MailSettings.SectionName
    // để tránh lỗi khi đổi tên section trong config
    builder.Configuration.GetSection(MailSettings.SectionName)
);

// khai báo các service trong module auth để có thể inject vào api mà không cần reference đến project auth
// auth dependency
Auth.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// notification dependency
Notification.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// search dependency
Search.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// add elasticsearch service
builder.AddElasticsearchClient("elastic-hanzi");

// add in-memory event bus
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

builder.Services.AddHttpClient<IOcrClient, OcrClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<OcrServiceOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});


var key = Encoding.ASCII.GetBytes(Constants.JWT_SECRET_KEY);
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // MapInboundClaims:
        // - Mặc định (true): JwtSecurityTokenHandler sẽ tự động chuyển đổi các claim type ngắn (như "name", "role")
        //   thành các URI dài chuẩn .NET (ClaimTypes.Name, ClaimTypes.Role, v.v.).
        //   Ví dụ: "role" → "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        // - Khi đặt false: giữ nguyên tên claim trong JWT (ví dụ: "name", "role").
        //   Điều này giúp token ngắn hơn, dễ đọc, và khớp với NameClaimType/RoleClaimType bạn cấu hình bên dưới.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ValidateIssuerSigningKey: Bắt buộc token phải có chữ ký hợp lệ (bảo mật).
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // ValidateIssuer: Có kiểm tra "issuer" trong token không? (ai phát hành token)
            // Nếu false: chấp nhận mọi issuer.
            ValidateIssuer = true,
            ValidIssuer = Constants.JWT_ISSUER, // Phải khớp với Issuer khi tạo token

            // ValidateAudience: Có kiểm tra "audience" trong token không? (token dành cho ai)
            // Nếu false: chấp nhận mọi audience.
            ValidateAudience = false,

            // NameClaimType: Chỉ định claim nào trong JWT sẽ được dùng làm User.Identity.Name.
            // Nếu bạn dùng "name" khi tạo token, phải set NameClaimType = "name".
            NameClaimType = "name",
            // RoleClaimType: Chỉ định claim nào trong JWT sẽ được dùng cho [Authorize(Roles = ...)].
            // Nếu bạn dùng "role" khi tạo token, phải set RoleClaimType = "role".
            RoleClaimType = "role"
            // "sub" không cần cấu hình ở đây — đọc trực tiếp qua User.Claims.FirstOrDefault(c => c.Type == "sub")
        };
        // Đọc JWT từ HttpOnly cookie thay vì Authorization header
        options.Events = new JwtBearerEvents // Cấu hình sự kiện để đọc token từ cookie
        {
            OnMessageReceived = context =>
            {
                // ưu tiên đọc token từ cookie, nếu không có mới đọc từ header (áp dụng cho mobile app hoặc client gửi token qua header)
                if (context.Request.Cookies.ContainsKey(ConfigureCookieSettings.IdentifierCookieName))
                {
                    // Nếu có cookie chứa JWT, lấy token từ cookie
                    context.Token = context.Request.Cookies[ConfigureCookieSettings.IdentifierCookieName];
                }
                else
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        // Nếu có Authorization header và bắt đầu bằng "Bearer ", lấy token từ header
                        // áp dụng cho mobile app hoặc client gửi token qua header
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

    /*
        var redisConnectionString = configuration["Redis:ConnectionString"];
        services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "TodoListLegend:"; // instance name để phân biệt với các ứng dụng khác nếu cùng dùng Redis
            });
            services.AddSingleton(typeof(ICache<>), typeof(RedisCacheAdapter<>));
     */
// add redis client
builder.AddRedisClient("redis-hanzi");



var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Enable CORS before routing
app.UseCors("AllowFrontend");

// add auth api trước để có thể sử dụng cookie authentication trong api sau (nếu có)
app.MapAuthApi();
app.MapOcrApi();
app.MapSearchApi();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "hanzi-anhvu-hsk-api" }));

app.Run();
