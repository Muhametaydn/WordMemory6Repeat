using System;
using System.ComponentModel.DataAnnotations;

namespace WordMemoryApp.Models
{
    /// <summary>Her sınav sorusunun sonucunu tutar.</summary>
    public class QuestionAttempt
    {
        [Key] public long AttemptID { get; set; }

        public int UserID { get; set; }
        public int WordID { get; set; }
        public QuestionType QuestionType { get; set; }

        public bool IsCorrect { get; set; }
        public DateTime AskedAtUtc { get; set; }

        // navigation
        public User? User { get; set; }
        public Word? Word { get; set; }
    }
}
