using System;

namespace Api.Models
{
    public class PermissionView
    {
        public int Id { get; set; }
        
        // Foreign Keys
        public int PermissionId { get; set; }
        public int ViewId { get; set; }

        // Navigation
        public Permission Permission { get; set; } = null!;
        public View View { get; set; } = null!;
    }
}
