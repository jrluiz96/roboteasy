namespace Api.Contracts.Requests;

public record ChatStartRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Cpf
);
