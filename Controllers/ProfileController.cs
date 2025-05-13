using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordMemoryApp.Data;
using WordMemoryApp.Models;

namespace WordMemoryApp.Controllers;

[Route("Profile")]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) => _db = db;

    // GET /Profile
    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(User.FindFirst("UserID")!.Value);

        var s = await _db.UserSettings
                         .FirstOrDefaultAsync(x => x.UserID == userId)
                 ?? new UserSettings { UserID = userId };

        ViewBag.Options = BuildOptions(userId);
        return View(s);
    }

    // POST /Profile
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserSettings form)
    {
        int userId = int.Parse(User.FindFirst("UserID")!.Value);   // oturumdan

        if (!ModelState.IsValid)
        {
            ViewBag.Options = BuildOptions(userId);
            return View(form);
        }

        var s = await _db.UserSettings.FindAsync(userId);

        if (s == null)                         // ilk kez ayar kaydediliyor
        {
            s = new UserSettings
            {
                UserID = userId,
                NewWordTarget = form.NewWordTarget
            };
            _db.UserSettings.Add(s);
        }
        else                                   // güncelleme
        {
            s.NewWordTarget = form.NewWordTarget;
        }

        await _db.SaveChangesAsync();
        TempData["msg"] = "Ayar kaydedildi.";
        return RedirectToAction("Index");
    }


    private IEnumerable<int> BuildOptions(int userId)
    {
        // Kullanıcının henüz öğrenmediği kelime sayısı
        int max = _db.Words.Count(w => !_db.UserWordProgresses
                            .Any(p => p.UserID == userId && p.WordID == w.WordID && p.IsLearned));

        if (max < 5) max = 5;                        // min 5
        if (max > 50) max = 50;                      // üst sınır

        for (int i = 5; i <= max; i += 5)
            yield return i;

        // max listeye dahil değilse son seçenek olarak ekle
        if (max % 5 != 0) yield return max;
    }

    // GET /Profile/AnalysisReport
    [HttpGet("AnalysisReport")]
    public async Task<IActionResult> AnalysisReport()
    {
        int userId = int.Parse(User.FindFirst("UserID")!.Value);

        // 1) Soru tiplerine göre istatistikler
        var stats = await _db.QuestionAttempts
            .Where(a => a.UserID == userId)
            .GroupBy(a => a.QuestionType)
            .Select(g => new QuestionTypeStat
            {
                QuestionType = g.Key,
                TotalAsked = g.Count(),
                CorrectCount = g.Count(a => a.IsCorrect)
            })
            .ToListAsync();

        // 2) 6× doğru bildiği için IsLearned = true olan kelimeler
        var learnedWords = await _db.UserWordProgresses
            .Where(p => p.UserID == userId && p.IsLearned)
            .Include(p => p.Word)
            .Select(p => p.Word!.EngWordName)
            .ToListAsync();

        var vm = new AnalysisReportViewModel
        {
            QuestionTypeStats = stats,
            FullyLearnedWords = learnedWords
        };

        return View(vm);
    }

    // …diğer aksiyonlar (Index, POST Index, BuildOptions vb.)

}
