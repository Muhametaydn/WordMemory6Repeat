using System.ComponentModel.DataAnnotations;

namespace WordMemoryApp.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; } = null!;   // ← zorunlu kullanıcı adı

        [Required, EmailAddress]
        public string Email { get; set; } = null!;      // ← zorunlu e-posta

        public string PasswordHash { get; set; } = null!;
    }
}
