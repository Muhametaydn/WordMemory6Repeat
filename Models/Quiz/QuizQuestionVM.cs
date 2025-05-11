using WordMemoryApp.Models;

namespace WordMemoryApp.Models.Quiz;

public class QuizQuestionVM
{
    public int WordID { get; set; }
    public QuestionType QuestionType { get; set; }

    public string? EngWord { get; set; }
    public string? TurkWord { get; set; }         // kontrol için gizli
    public string? PicturePath { get; set; }
    public string? SentenceHtml { get; set; }

    // Kullanıcının girdiği cevap
    public string? UserAnswer { get; set; }
}
