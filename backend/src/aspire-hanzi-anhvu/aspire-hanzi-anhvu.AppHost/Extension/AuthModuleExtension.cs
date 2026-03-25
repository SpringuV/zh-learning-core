namespace AspireHanziAnhVu.AppHost.Extension;

public static class AuthModuleExtension
{

    public static IDistributedApplicationBuilder AddAuthModule(this IDistributedApplicationBuilder builder, string name)
    {
        // Đăng ký các service liên quan đến module auth ở đây
        // Ví dụ: builder.Services.AddScoped<IIdentityService, IdentityService>();


        return builder;
    }
}
