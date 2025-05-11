using System;
using System.ComponentModel.DataAnnotations;

namespace WordMemoryApp.Models
{
    /// <summary>Her kullanıcı-kelime için doğru seri takibi.</summary>
    public class UserWordProgress
    {
        [Key] public int UserWordProgressID { get; set; }

        public int UserID { get; set; }
        public int WordID { get; set; }

        public byte CorrectStreak { get; set; }  // art arda doğru sayısı (0-6)
        public bool IsLearned { get; set; }  // 6 olduğunda true

        // navigation
        public User? User { get; set; }
        public Word? Word { get; set; }
    }
}
