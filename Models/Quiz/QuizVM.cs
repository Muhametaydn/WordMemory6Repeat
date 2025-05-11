using System.Collections.Generic;

namespace WordMemoryApp.Models.Quiz;

public class QuizVM
{
    public List<QuizQuestionVM> Questions { get; set; } = new();
}