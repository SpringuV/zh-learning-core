using Shared.Infrastructure.Outbox;

namespace Auth.Infrastructure.Identity;

public class AuthIdentityDbContext: IdentityDbContext<AuthApplicationUser>
{
    public AuthIdentityDbContext(DbContextOptions<AuthIdentityDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<AuthOutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Gọi base trước để IdentityDbContext tự cấu hình các bảng mặc định:
        // AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, v.v.
        base.OnModelCreating(builder);

        builder.Entity<AuthOutboxMessage>(entity =>
        {
            entity.ConfigureOutboxMessage("OutboxMessages");
        });

        // ── Quan hệ: ApplicationUser ←→ IdentityUserRole ──────────────
        // Đọc theo hướng: "ApplicationUser có NHIỀU UserRoles,
        //                  mỗi UserRole thuộc về MỘT ApplicationUser"
        //
        //  ApplicationUser          IdentityUserRole<string>
        //  ───────────────          ────────────────────────
        //  Id (PK)       ◄──────── UserId (FK)
        //  UserName                 RoleId
        //  ...                      (bảng AspNetUserRoles)
        //
        builder.Entity<AuthApplicationUser>()
            // HasMany: "ApplicationUser có NHIỀU UserRoles"
            // → 1 user có thể có nhiều role (Admin, User, Manager...)
            .HasMany(u => u.UserRoles)

            // WithOne(): "mỗi UserRole thuộc về MỘT user"
            // → không truyền lambda vì IdentityUserRole KHÔNG có
            //   navigation property ngược lại (.User) theo mặc định
            //   (nếu có thì viết: .WithOne(ur => ur.User))
            .WithOne() // là bảng trung gian nên mới là with one, •	User ↔ UserRole ↔ Role

            // HasForeignKey: cột nào trong IdentityUserRole là khóa ngoại
            .HasForeignKey(ur => ur.UserId)

            // IsRequired: UserId không được null (bắt buộc phải thuộc về 1 user)
            .IsRequired();

        builder.Entity<AuthApplicationUser>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique(false);

        // ── Quan hệ: RefreshToken ←→ ApplicationUser ──────────────────
        // Đọc theo hướng: "RefreshToken thuộc về MỘT ApplicationUser,
        //                  1 ApplicationUser có THỂ có NHIỀU RefreshToken"
        //
        //  RefreshToken             ApplicationUser
        //  ────────────             ───────────────
        //  Id (PK)                  Id (PK)
        //  UserId (FK) ────────►   UserName
        //  Token                   ...
        //  IsRevoked
        //
        builder.Entity<RefreshToken>(entity =>
        {
            // Đảm bảo Token là duy nhất trong DB (không thể có 2 refresh token giống nhau)
            entity.HasIndex(t => t.Token).IsUnique();

            entity
                // HasOne: "RefreshToken thuộc về MỘT ApplicationUser"
                // → mỗi refresh token chỉ gắn với 1 user cụ thể
                .HasOne(t => t.AuthUser)

                // WithMany(): "ApplicationUser có THỂ có NHIỀU RefreshToken"
                // → không truyền lambda vì ApplicationUser không có
                //   navigation property ngược lại (.RefreshTokens)
                //   (nếu có thì viết: .WithMany(u => u.RefreshTokens))
                .WithMany()

                // HasForeignKey: cột UserId trong RefreshToken là khóa ngoại
                .HasForeignKey(t => t.UserId)

                // OnDelete Cascade: khi xóa User → tự động xóa hết RefreshToken của user đó
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
