namespace Auth.Contracts.DTOs;

public record UserActiveAcountRequest(string Email, string ActivationCode, string EmailActivationCode);
