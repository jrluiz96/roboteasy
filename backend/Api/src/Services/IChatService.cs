using Api.Contracts.Requests;
using Api.Contracts.Responses;

namespace Api.Services;

public interface IChatService
{
    /// <summary>
    /// Cria ou busca o cliente pelo email e retorna um token de acesso ao WebSocket
    /// </summary>
    Task<ChatStartResponse> StartAsync(ChatStartRequest request);
}
