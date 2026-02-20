namespace Api.Hubs;

/// <summary>
/// Nomes dos eventos trafegados pelo SignalR.
/// Usar estas constantes garante consistência entre Hub e clientes.
/// </summary>
public static class ChatEvents
{
    // Conexão
    public const string UserOnline  = "user:online";
    public const string UserOffline = "user:offline";

    // Mensagens
    public const string ReceiveMessage = "message:receive";

    // Typing
    public const string Typing     = "typing:start";
    public const string StopTyping = "typing:stop";

    // Leitura
    public const string MessageRead = "message:read";

    // Conversa
    public const string ConversationFinished  = "conversation:finished";
    public const string ConversationInvited   = "conversation:invited";
    public const string ConversationCreated   = "conversation:created";
    public const string AttendantLeft         = "attendant:left";
}
