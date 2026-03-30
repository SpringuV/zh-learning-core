namespace Auth.Contracts.DTOs;

public record ChangePasswordRequest(string UserId,string OldPassword, string NewPassword);
