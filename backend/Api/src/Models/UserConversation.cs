using System;

namespace Api.Models
{
    /// <summary>
    /// Relaciona atendentes (users) com conversas
    /// Uma conversa pode ter múltiplos atendentes (transferência)
    /// </summary>
    public class UserConversation
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public long ConversationId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
        
        /// <summary>
        /// Logs de eventos (transferência, pausa, etc) em JSON
        /// </summary>
        public string? Events { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Conversation Conversation { get; set; } = null!;
    }
}
