namespace Api.Contracts.Requests;

public record RegisterRequest(string Name, string Username, string Email, string Password);
