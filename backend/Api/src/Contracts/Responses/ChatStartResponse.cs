namespace Api.Contracts.Responses;

public record ChatStartResponse(
    long ClientId,
    string ClientToken
);
