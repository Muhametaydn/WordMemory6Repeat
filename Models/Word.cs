namespace WordMemoryApp.Models
{
    public class Word
    {
        public int WordID { get; set; }
        public string EngWordName { get; set; } = null!;
        public string TurWordName { get; set; } = null!;
        public string? Picture { get; set; }
        // Yeni alan: hangi kullanıcı eklemiş
        public int OwnerId { get; set; }

        // Navigation
        public User? Owner { get; set; }
        public List<WordSample> Samples { get; set; } = new();
    }
}
