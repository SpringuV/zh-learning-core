using HanziAnhVu.Shared.Domain;

namespace HanziAnhVu.Modules.Users.Domain;
// Auth.Domain (Sạch, không dính dáng framework)
public class UserAggregate : BaseAggregateRoot
{
    public Guid Id { get; private set; } // private set để tránh việc thay đổi email sau khi đã tạo user
 

    protected UserAggregate() { } // constructor protected để chỉ cho phép tạo user thông qua factory method

    // factory method để tạo user mới, đảm bảo rằng tất cả các trường cần thiết được khởi tạo đúng cách
    // để static để có thể gọi mà không cần instance của UserAggregate, và trả về một instance mới của UserAggregate
    public static UserAggregate Create(string email, string username, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new UserAggregate
        {
            Id = Guid.NewGuid(),
            //Email = email,
            //UserName = username,
            //PasswordHash = passwordHash,
            //CreatedAt = DateTime.UtcNow,
            //UpdatedAt = DateTime.UtcNow
        };
        // Fire domain event — các handler khác lắng nghe
        //user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, user.Email));

        return user;
    }

    

}
