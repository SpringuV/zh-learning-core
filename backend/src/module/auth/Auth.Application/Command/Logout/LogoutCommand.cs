
namespace Auth.Application.Command.Logout;

public record LogoutCommand(string RefreshToken): IRequest<UserLoggoutResponse>; // Irequest <string> vì sau khi logout thành công sẽ trả về message dạng string, nếu có lỗi sẽ ném exception và được middleware xử lý thành response lỗi
