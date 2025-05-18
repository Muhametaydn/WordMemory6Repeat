namespace WordMemoryApp.Models
{
    /// <summary>Aktif Wordle oyununun geçici durumu (yalnızca Session’da tutulur).</summary>
    public record PuzzleState(string Target, int Attempts, bool IsFinished)
    {
        /// <summary>Her oyun daima 5 tahmin hakkı (5 satır).</summary>
        public const int MaxAttempts = 5;
    }
}
