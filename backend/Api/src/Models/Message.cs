using System;

namespace Api.Models
{
    public enum MessageType
    {
        Text = 1,
        Image = 2,
        File = 3,
        Audio = 4,
        Video = 5,
        System = 6  // Mensagens automáticas do sistema
    }

    /// <summary>
    /// Mensagem dentro de uma conversa
    /// </summary>
    public class Message
    {
        public long Id { get; set; }
        public long ConversationId { get; set; }
        
        /// <summary>
        /// Cliente que enviou (null se foi atendente)
        /// </summary>
        public long? ClientId { get; set; }
        
        /// <summary>
        /// Atendente que enviou (null se foi cliente)
        /// </summary>
        public int? UserId { get; set; }
        
        public MessageType Type { get; set; } = MessageType.Text;
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// URL/caminho do arquivo (para img, file, audio, video)
        /// Melhor que base64 para performance
        /// </summary>
        public string? FileUrl { get; set; }
        
        /// <summary>
        /// Nome original do arquivo
        /// </summary>
        public string? FileName { get; set; }
        
        /// <summary>
        /// Tamanho em bytes
        /// </summary>
        public long? FileSize { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Conversation Conversation { get; set; } = null!;
        public Client? Client { get; set; }
        public User? User { get; set; }
    }
}
