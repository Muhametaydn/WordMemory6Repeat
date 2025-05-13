using System.ComponentModel.DataAnnotations;

namespace WordMemoryApp.Models
{
    public class UserSettings
    {
        [Key] public int UserID { get; set; }

        [Range(5, 50)]
        public int NewWordTarget { get; set; } = 10;

        public User? User { get; set; }
    }
}
