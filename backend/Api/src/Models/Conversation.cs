using System;

namespace Api.Models
{
    /// <summary>
    /// Conversa/sessão de chat entre cliente e atendentes
    /// </summary>
    public class Conversation
    {
        public long Id { get; set; }
        public long ClientId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
        
        /// <summary>
        /// Tempo total de atendimento em segundos
        /// </summary>
        public int? AttendanceTime { get; set; }

        // Navigation
        public Client Client { get; set; } = null!;
        public ICollection<UserConversation> UserConversations { get; set; } = new List<UserConversation>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
