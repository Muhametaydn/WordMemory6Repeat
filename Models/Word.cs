namespace WordMemoryApp.Models
{
    public class Word
    {
        public int WordID { get; set; }
        public string EngWordName { get; set; } = null!;
        public string TurWordName { get; set; } = null!;
        public string? Picture { get; set; }
        public List<WordSample> Samples { get; set; } = new();
    }
}
