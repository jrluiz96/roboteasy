using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<View> Views => Set<View>();
    public DbSet<PermissionView> PermissionViews => Set<PermissionView>();
    
    // Chat
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email);
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Token).HasMaxLength(500);
            entity.Property(u => u.WsConn).HasMaxLength(255);
            
            // Soft delete filter
            entity.HasQueryFilter(u => u.DeletedAt == null);
            
            // Relationship
            entity.HasOne(u => u.Permission)
                  .WithMany(p => p.Users)
                  .HasForeignKey(u => u.PermissionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.HasQueryFilter(p => p.DeletedAt == null);
        });

        // View
        modelBuilder.Entity<View>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Name).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Route).HasMaxLength(255).IsRequired();
            entity.HasQueryFilter(v => v.DeletedAt == null);
        });

        // PermissionView (many-to-many)
        modelBuilder.Entity<PermissionView>(entity =>
        {
            entity.HasKey(pv => pv.Id);
            
            entity.HasOne(pv => pv.Permission)
                  .WithMany(p => p.PermissionViews)
                  .HasForeignKey(pv => pv.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pv => pv.View)
                  .WithMany(v => v.PermissionViews)
                  .HasForeignKey(pv => pv.ViewId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== CHAT ==========

        // Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
            entity.Property(c => c.WsConn).HasMaxLength(255);
            entity.Property(c => c.Cpf).HasMaxLength(14);
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Email).HasMaxLength(255);
            
            // Índices para busca rápida
            entity.HasIndex(c => c.Cpf);
            entity.HasIndex(c => c.Phone);
            entity.HasIndex(c => c.Email);
            entity.HasIndex(c => c.CreatedAt);
        });

        // Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            // Índices para performance
            entity.HasIndex(c => c.ClientId);
            entity.HasIndex(c => c.CreatedAt);
            entity.HasIndex(c => c.FinishedAt);
            // Índice composto: conversas ativas de um cliente
            entity.HasIndex(c => new { c.ClientId, c.FinishedAt });
            
            entity.HasOne(c => c.Client)
                  .WithMany(cl => cl.Conversations)
                  .HasForeignKey(c => c.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // UserConversation
        modelBuilder.Entity<UserConversation>(entity =>
        {
            entity.HasKey(uc => uc.Id);
            entity.Property(uc => uc.Events).HasColumnType("jsonb");
            
            // Índices para buscar conversas de um atendente
            entity.HasIndex(uc => uc.UserId);
            entity.HasIndex(uc => uc.ConversationId);
            // Índice composto: atendimentos ativos de um user
            entity.HasIndex(uc => new { uc.UserId, uc.FinishedAt });
            
            entity.HasOne(uc => uc.User)
                  .WithMany(u => u.UserConversations)
                  .HasForeignKey(uc => uc.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uc => uc.Conversation)
                  .WithMany(c => c.UserConversations)
                  .HasForeignKey(uc => uc.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Content).IsRequired();
            entity.Property(m => m.FileUrl).HasMaxLength(500);
            entity.Property(m => m.FileName).HasMaxLength(255);
            
            // Índices críticos para chat (alta performance)
            entity.HasIndex(m => m.ConversationId);
            entity.HasIndex(m => m.CreatedAt);
            // Índice composto: mensagens de uma conversa ordenadas
            entity.HasIndex(m => new { m.ConversationId, m.CreatedAt });
            
            entity.HasOne(m => m.Conversation)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Client)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(m => m.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.User)
                  .WithMany(u => u.Messages)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
