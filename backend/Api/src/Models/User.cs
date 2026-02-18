using System;

namespace Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? WsConn { get; set; }
        
        // GitHub OAuth
        public long? GitHubId { get; set; }
        public string? GitHubLogin { get; set; }
        public string? AvatarUrl { get; set; }
        
        // Foreign Key
        public int PermissionId { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? SessionAt { get; set; }

        // Navigation
        public Permission Permission { get; set; } = null!;
        public ICollection<UserConversation> UserConversations { get; set; } = new List<UserConversation>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
