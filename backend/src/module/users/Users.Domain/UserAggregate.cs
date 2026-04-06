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

    // AI Usage Tracking (Local to Users module)
    // Kiểm tra quota realtime — tần suất đọc rất cao
    public int AiMessagesUsedToday { get; private set; } = 0;
    public DateTime AiMessagesUsedTodayDate { get; private set; } = DateTime.UtcNow.Date;
    public DateTime AiUsageResetAt { get; private set; }


    // Subscription Cache (Replicated from Payment module via Events)
    // Source of Truth: Payment.user_subscription
    // Updated via: SubscriptionChangedIntegrationEvent → Users module listens
    // Purpose: Cache tier/status locally để check AI quota dựa vào subscription
    public SubscriptionTierEnum CurrentTier { get; private set; } = SubscriptionTierEnum.Free;
    public SubscriptionStatusEnum SubscriptionStatus { get; private set; } = SubscriptionStatusEnum.Pending;

    // Computed property: Lấy limit dựa vào CurrentTier
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

    public void UpdateSubscription(SubscriptionStatusEnum status, SubscriptionTierEnum tier, DateTime resetAt)
    {
        // Updates cache từ Payment module event (SubscriptionChangedIntegrationEvent)
        SubscriptionStatus = status;
        CurrentTier = tier;
        AiUsageResetAt = resetAt;
    }

    // recordAiMessageUsage sẽ được gọi mỗi khi user sử dụng tính năng AI, 
    // để tăng counter và kiểm tra xem có cần reset quota hàng ngày hay không
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
        // Check if need to reset daily quota
        if (DateTime.UtcNow >= AiUsageResetAt)
        {
            AiMessagesUsedToday = 0;
            AiMessagesUsedTodayDate = DateTime.UtcNow.Date;
            AiUsageResetAt = DateTime.UtcNow.AddDays(1);
        }

        // Check subscription status (from Payment cache) + daily quota (local tracking)
        return SubscriptionStatus == SubscriptionStatusEnum.Active && AiMessagesUsedToday < AiMessagesLimitPerDay;
    }

    private static int GetAiMessagesLimitByTier(SubscriptionTierEnum tier) => tier switch
    {
        // Limits based on subscription tier (replicated from Payment.subscription_plan)
        SubscriptionTierEnum.Free => 3,
        SubscriptionTierEnum.Pro => 15,
        SubscriptionTierEnum.Premium => 30,
        _ => 3
    };

}
