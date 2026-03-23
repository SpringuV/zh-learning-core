namespace Auth.Application.Command.Register;

// input dto for registering a user
public record RegisterUserCommand(string Email, string UserName, string Password) : IRequest<Guid>;
// IRequest<Guid> có nghĩa là khi command này được xử lý xong, nó sẽ trả về một Guid (có thể là userId của user mới được tạo)
