using System;

namespace WordMemoryApp.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
