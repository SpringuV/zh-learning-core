var builder = WebApplication.CreateBuilder(args);
// builder.AddElasticOpenTelemetry();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var redisConnectionString =
    builder.Configuration.GetConnectionString("redis-hanzi")
    ?? builder.Configuration["Redis:ConnectionString"];

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException("Missing Redis connection string. Configure ConnectionStrings__redis-hanzi or Redis:ConnectionString.");
}

// Keep one canonical connection key so both AddStackExchangeRedisCache 
// and AddRedisClient("redis-hanzi") work.
builder.Configuration["ConnectionStrings:redis-hanzi"] = redisConnectionString;

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "hanzi-anhvu:";
});

// Add MediatR pipeline behaviors (validation, caching, logging)
// Đăng ký các pipeline behaviors cho MediatR, đảm bảo chúng được áp dụng 
// cho tất cả các request handlers trong ứng dụng.
// phải đặt sau khi đăng ký các handlers trong từng module để đảm bảo pipeline behaviors được áp dụng cho tất cả handlers.
builder.Services.AddMediatRPipelineBehaviors();
// add redis client
builder.AddRedisClient("redis-hanzi");
// Configure CORS to allow frontend
var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
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

// classroom dependency
Classroom.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// lesson dependency
Lesson.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// users dependency
Users.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// notification dependency
Notification.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);

// search dependency
Search.Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services, builder);

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

builder.Services.AddScoped(typeof(IAppLogger<>), typeof(LoggingAdapter<>));

// Trust reverse-proxy headers from Nginx (X-Forwarded-Proto/Host/For).
// Keep KnownNetworks/KnownProxies restricted in stricter environments if possible.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


var key = Encoding.ASCII.GetBytes(Auth.Infrastructure.Constants.JWT_SECRET_KEY);
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
            ValidateLifetime = true, // Bắt buộc token phải có thời gian sống hợp lệ (không hết hạn).
            // ValidateIssuerSigningKey: Bắt buộc token phải có chữ ký hợp lệ (bảo mật).
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // ValidateIssuer: Có kiểm tra "issuer" trong token không? (ai phát hành token)
            // Nếu false: chấp nhận mọi issuer.
            ValidateIssuer = true,
            ValidIssuer = Auth.Infrastructure.Constants.JWT_ISSUER, // Phải khớp với Issuer khi tạo token

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




var app = builder.Build();

// Apply forwarded headers before auth/cookie/redirect logic so request scheme is accurate behind Nginx.
app.UseForwardedHeaders();

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Enable CORS before routing
app.UseCors(MyAllowSpecificOrigins);

// Enable auth middleware so JWT cookie/header can populate HttpContext.User
app.UseAuthentication();
app.UseAuthorization();

// add auth api trước để có thể sử dụng cookie authentication trong api sau (nếu có)
app.MapAuthApi();
app.MapOcrApi();
app.MapSearchApi();
app.MapLessonApi();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "hanzi-anhvu-hsk-api" }));

app.Run();

/*
    1) Browser -> Nginx: HTTPS (public SSL cert ở Nginx).
    2) Nginx -> Kestrel: HTTP nội bộ (container network/private network) là mô hình phổ biến.
    3) Không bắt buộc cert riêng cho Kestrel, trừ khi bạn cần end-to-end TLS/compliance.
    4) Nginx cần forward: X-Forwarded-Proto, X-Forwarded-Host, X-Forwarded-For.
    5) App đã bật UseForwardedHeaders() để xử lý đúng scheme/callback/cookie khi chạy sau proxy.

 Publish/Deploy note (Nginx + Kestrel):

 1) Browser -> Nginx: HTTPS (SSL cert đặt ở Nginx).
 2) Nginx -> Kestrel: thường là HTTP nội bộ (private network/container network).
     => Thông thường KHÔNG cần cert public riêng cho Kestrel.

 3) Nếu bật HTTPS ở Kestrel trong production thì phải cấu hình thêm cert cho Kestrel.
     Chỉ cần khi bạn muốn end-to-end TLS hoặc có yêu cầu compliance.

 4) Khi chạy sau reverse proxy, cần forward các header:
     - X-Forwarded-Proto
     - X-Forwarded-Host
     - X-Forwarded-For
     để app biết request gốc là HTTPS và tránh redirect/cookie sai scheme.

 5) UseHttpsRedirection vẫn dùng được, nhưng phải đảm bảo forwarded headers cấu hình đúng,
     nếu không có thể gặp redirect loop hoặc callback URL bị nhảy sang http.

    Mô hình bạn mô tả là mô hình chuẩn, tiết kiệm nhất:
    Internet -> Nginx: HTTPS
    Nginx -> Next container: HTTP nội bộ
    Nginx -> .NET container: HTTP nội bộ
    Các container nói chuyện nhau qua Docker network private, không cần HTTPS giữa các container.
    Nginx chịu trách nhiệm SSL termination, load balancing, và forwarding header để .NET app biết request gốc là HTTPS.

    Với mô hình đó, bạn không bắt buộc mua cert cho từng service nội bộ.
    Chỉ cần cert ở Nginx public edge.
    Bên trong network nội bộ để HTTP là bình thường.
Về tiền cert:
    Lets Encrypt: miễn phí, rất phổ biến cho production.
    Cert trả phí DV: thường khoảng 10 đến 100 USD mỗi năm.
    OV/EV: có thể từ vài trăm đến hơn 1000 USD mỗi năm tùy nhà cung cấp.
    Nếu ngân sách thấp, dùng Lets Encrypt là lựa chọn hợp lý và đủ dùng cho đa số hệ thống.
Câu hỏi quan trọng của bạn: các file token-manager/hook đã xóa còn quan trọng không khi publish bằng container network?
Câu trả lời: không, vẫn không quan trọng trong kiến trúc hiện tại.
    Lý do không nằm ở container network, mà nằm ở HttpOnly cookie.
    Khi cookie là HttpOnly, JavaScript phía client không đọc được token dù bạn chạy localhost hay production.
    Vì vậy các file cũ kiểu đọc token từ client cookie vẫn không phù hợp.
Cái cần giữ trong production:
    Flow restore session và refresh ở server-side như bạn đang làm tại route.ts.
    Interceptor HTTP theo hướng cookie-based refresh ở http.ts.
    Middleware auth/authorization backend ở Program.cs.
Checklist để không lỗi khi lên Nginx:
    Nginx gửi đủ X-Forwarded-Proto, X-Forwarded-Host, X-Forwarded-For
    App trust forwarded headers
    Cookie auth đặt HttpOnly + Secure trong production
    URL cấu hình ngoài cùng dùng domain HTTPS thật, không dùng localhost


// mẫy nginx
     đây là mẫu tối giản để bạn deploy đúng hướng bạn muốn: chỉ Nginx có HTTPS, container nội bộ chạy HTTP.

    Kiến trúc đích:

    Browser -> Nginx: HTTPS
    Nginx -> Next container: HTTP nội bộ
    Nginx -> .NET API container: HTTP nội bộ
    Không cần cert trả phí cho từng container nội bộ
    Mẫu Nginx (1 domain, route / vào Next, /api vào .NET):

    server {
        listen 80;
        server_name your-domain.com;
        return 301 https://$host$request_uri;
        }

        server {
        listen 443 ssl http2;
        server_name your-domain.com;
        ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
        ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

        client_max_body_size 20m;

        location / {
            proxy_pass http://next-app:3000;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
        }

        location /api/ {
            proxy_pass http://dotnet-api:8080/;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }
    }

    Phần .NET cần thêm để hiểu đúng HTTPS gốc qua proxy
    (chỉnh trong Program.cs):

    using Microsoft.AspNetCore.HttpOverrides;

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
    options.ForwardedHeaders =
    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    });

    var app = builder.Build();
    app.UseForwardedHeaders();

    Phần Next env production
    (chỉnh trong .env hoặc biến môi trường runtime):

    NEXTAUTH_URL=https://your-domain.com
    NEXT_PUBLIC_SITE_URL=https://your-domain.com
    NEXT_PUBLIC_API_BASE_URL=https://your-domain.com/api
    Về chứng chỉ:

    Không đủ tiền mua cert vẫn ổn, dùng Let’s Encrypt là miễn phí.
    Bạn chỉ cần cert ở Nginx public edge.
    Nội bộ container network giữ HTTP là bình thường.
    Lưu ý quan trọng với flow auth của bạn:

    Các file token manager client đọc cookie đã xóa là đúng.
    Với HttpOnly cookie, restore/refresh server-side như hiện tại là đúng chuẩn production.
    Nginx chỉ là lớp transport HTTPS, không thay thế logic session restore của app.
*/
