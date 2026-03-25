namespace Auth.Contracts.DTOs;

public record LoginRequest(string Username, string Password, string TypeLogin);