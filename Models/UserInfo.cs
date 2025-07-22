using System;

namespace GoogleLogin.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string? GoogleId { get; set; } // Nullable
        public string? GitHubId { get; set; } // Nullable
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}