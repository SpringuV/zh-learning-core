using HanziAnhVu.Shared.Domain;

namespace Users.Domain;

public enum SubscriptionStatusEnum
{
    Pending = 0,
    Active = 1,
    Expired = 2,
    Cancelled = 3
}

public enum SubscriptionTierEnum
{
    Free = 0,
    Pro = 1,
    Premium = 2
}

// Auth.Domain (Sạch, không dính dáng framework)
public class UserAggregate : BaseAggregateRoot
{
    public Guid Id { get; private set; } // soft reference đến user trong auth service, tránh việc phải reference đến project auth
    public int CurrentHskLevel { get; private set; } = 0;
    public string? AvatarUrl { get; private set; } = null;

    // Ai usage tracking - info cho ngày hôm nay
    public int AiMessagesUsedToday { get; private set; } = 0;
    public DateTime AiMessagesUsedTodayDate { get; private set; } = DateTime.UtcNow.Date;
    public SubscriptionTierEnum CurrentTier { get; private set; } = SubscriptionTierEnum.Free;
    public DateTime AiUsageResetAt { get; private set; }

    // Subscription
    public SubscriptionStatusEnum SubscriptionStatus { get; private set; } = SubscriptionStatusEnum.Pending;

    public int AiMessagesLimitPerDay => GetAiMessagesLimitByTier(CurrentTier);

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

    public void UpdateSubscription(SubscriptionStatusEnum status, SubscriptionTierEnum tier, DateTime resetAt)
    {
        SubscriptionStatus = status;
        CurrentTier = tier;
        AiUsageResetAt = resetAt;
    }

    public void RecordAiMessageUsage()
    {
        // Check if need to reset
        if (DateTime.UtcNow >= AiUsageResetAt)
        {
            AiMessagesUsedToday = 0;
            AiMessagesUsedTodayDate = DateTime.UtcNow.Date;
            AiUsageResetAt = DateTime.UtcNow.AddDays(1);
        }

        AiMessagesUsedToday++;
    }

    public bool CanUseAiMessage()
    {
        // Check if need to reset
        if (DateTime.UtcNow >= AiUsageResetAt)
        {
            AiMessagesUsedToday = 0;
            AiMessagesUsedTodayDate = DateTime.UtcNow.Date;
            AiUsageResetAt = DateTime.UtcNow.AddDays(1);
        }

        return SubscriptionStatus == SubscriptionStatusEnum.Active && AiMessagesUsedToday < AiMessagesLimitPerDay;
    }

    private static int GetAiMessagesLimitByTier(SubscriptionTierEnum tier) => tier switch
    {
        SubscriptionTierEnum.Free => 3,
        SubscriptionTierEnum.Pro => 15,
        SubscriptionTierEnum.Premium => 30,
        _ => 3
    };

}
