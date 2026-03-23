namespace Auth.Infrastructure.Identity;
public class RefreshToken
{
    public int Id { get; set; }

    // Token ngẫu nhiên 64 bytes, lưu trong DB và HttpOnly cookie
    public string Token { get; set; } = string.Empty;

    // FK sang AspNetUsers
    public string UserId { get; set; } = string.Empty;
    public AuthApplicationUser AuthUser { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    // true khi user logout hoặc khi token bị rotate (thay thế bằng token mới)
    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
