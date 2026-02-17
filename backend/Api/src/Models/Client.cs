using System;

namespace Api.Models
{
    /// <summary>
    /// Cliente externo que inicia uma conversa no chat
    /// </summary>
    public class Client
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? WsConn { get; set; }
        public string? Cpf { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
