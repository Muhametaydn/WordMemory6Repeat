using Microsoft.AspNetCore.Mvc;
using WordMemoryApp.Services;
using WordMemoryApp.Models.Quiz;
using WordMemoryApp.Models;

namespace WordMemoryApp.Controllers;

public class QuizController : Controller
{
    private readonly QuizService _quiz;
    public QuizController(QuizService quiz) => _quiz = quiz;

    // GET: /Quiz/Take
    public async Task<IActionResult> Take(int count = 10)
    {
        // Örnek: userId cookie/claim'den
        int userId = int.Parse(User.FindFirst("UserID")!.Value);

        var dtoList = await _quiz.GenerateQuizAsync(userId, count);
        var vm = new QuizVM
        {
            Questions = dtoList.Select(d => new QuizQuestionVM
            {
                WordID = d.WordID,
                QuestionType = d.QuestionType,
                EngWord = d.EngWord,
                TurkWord = d.TurkWord,
                PicturePath = d.PicturePath,
                SentenceHtml = d.SentenceHtml
            }).ToList()
        };

        return View(vm);               // Views/Quiz/Take.cshtml
    }

    // POST: /Quiz/Submit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(QuizVM vm)
    {
        int userId = int.Parse(User.FindFirst("UserID")!.Value);

        int total = vm.Questions.Count;
        int correct = 0;

        foreach (var q in vm.Questions)
        {
            bool isCorrect = q.QuestionType switch
            {
                QuestionType.EngToTurk =>
                    string.Equals(q.UserAnswer?.Trim(), q.TurkWord, StringComparison.OrdinalIgnoreCase),

                QuestionType.PictureToEng =>
                    string.Equals(q.UserAnswer?.Trim(), q.EngWord, StringComparison.OrdinalIgnoreCase),

                QuestionType.SentenceToTurk =>
                    string.Equals(q.UserAnswer?.Trim(), q.TurkWord, StringComparison.OrdinalIgnoreCase),

                _ => false
            };

            if (isCorrect) correct++;

            await _quiz.RecordAnswerAsync(userId, q.WordID, isCorrect, q.QuestionType);
        }

        TempData["QuizScore"] = $"{correct} / {total}";
        return RedirectToAction("Index", "Home");   // sınav bitince ana sayfa
    }


}
