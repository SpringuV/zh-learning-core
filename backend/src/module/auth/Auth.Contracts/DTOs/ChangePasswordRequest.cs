namespace Auth.Contracts.DTOs;

public record ChangePasswordRequest(string OldPassword, string NewPassword);
