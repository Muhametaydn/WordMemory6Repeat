namespace WordMemoryApp.Models
{
    public enum QuestionType : byte
    {
        EngToTurk = 0,   // İngilizce kelime → Türkçe cevabı
        PictureToEng = 1,// Resim → İngilizce kelime
        SentenceToTurk = 2 // Cümlede altı çizili İngilizce kelime → Türkçe cevabı
    }
}
