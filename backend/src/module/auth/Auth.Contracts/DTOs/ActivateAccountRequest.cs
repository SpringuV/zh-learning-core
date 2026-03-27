namespace Auth.Contracts.DTOs;

public record ActivateAccountRequest(string Email, string Code);
