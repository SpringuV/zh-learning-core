namespace HanziAnhVuHsk.Api.Config
{
    /// <summary>
    /// Hằng số liên quan đến cookie JWT cho Web API.
    /// Cookie được set/delete thủ công trong api auth, và được đọc qua JwtBearer.OnMessageReceived trong Program.cs.
    /// </summary>
    public static class ConfigureCookieSettings
    {
        // Cookie HttpOnly chứa access token (JWT ngắn hạn, xác thực mỗi request)
        public const string IdentifierCookieName = "HanziAnhVu.Identity";

        // Cookie HttpOnly chứa refresh token (dài hạn, lưu trong DB, dùng để cấp lại access token)
        public const string RefreshTokenCookieName = "HanziAnhVu.Refresh";
    }
}
