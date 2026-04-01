using HanziAnhVu.Shared.Domain;

namespace Users.Domain;
// Auth.Domain (Sạch, không dính dáng framework)
public class UserAggregate : BaseAggregateRoot
{
    public Guid Id { get; private set; } // soft reference đến user trong auth service, tránh việc phải reference đến project auth
    public int LongestStreak { get; private set; } = 0; // private set để tránh việc thay đổi username sau khi đã tạo user
    public int TotalXp { get; private set; } = 0;
    public int CurrentHskLevel { get; private set; } = 0;
    public int StreakCount { get; private set; } = 0;
    public string? AvatarUrl { get; private set; } = null;


    protected UserAggregate() { } // constructor protected để chỉ cho phép tạo user thông qua factory method

    // factory method để tạo user mới, đảm bảo rằng tất cả các trường cần thiết được khởi tạo đúng cách
    // để static để có thể gọi mà không cần instance của UserAggregate, và trả về một instance mới của UserAggregate
    public static UserAggregate Create(Guid idUser)
    {
        if (idUser == Guid.Empty) throw new ArgumentException("idUser cannot be empty", nameof(idUser));

        var user = new UserAggregate
        {
            Id = idUser,
        };
        // Fire domain event — các handler khác lắng nghe
        //user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, user.Email));

        return user;
    }

    public void UpdateProfile(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        // Fire domain event — các handler khác lắng nghe
        //user.AddDomainEvent(new UserProfileUpdatedDomainEvent(user.Id, user.Email));
    }

    public void UpdateStreak(int newStreak)
    {
        if (newStreak < 0) throw new ArgumentException("Streak cannot be negative", nameof(newStreak));
        StreakCount = newStreak;
        if (newStreak > LongestStreak)
        {
            LongestStreak = newStreak;
        }
    }

    public void AddXp(int xp)
    {
        if (xp < 0) throw new ArgumentException("XP cannot be negative", nameof(xp));
        TotalXp += xp;
        // Cập nhật HSK level dựa trên XP, ví dụ: mỗi 1000 XP tăng 1 level
        CurrentHskLevel = TotalXp / 1000;
    }
}
