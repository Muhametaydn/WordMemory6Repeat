using Microsoft.EntityFrameworkCore;
using WordMemoryApp.Data;
using WordMemoryApp.Models;

namespace WordMemoryApp.Services;

public class QuizService
{
    private readonly AppDbContext _db;
    private const int DefaultQuestionCount = 10;

    public QuizService(AppDbContext db) => _db = db;

    // ---------- Sınav oluştur ----------
    public async Task<List<QuizQuestionDto>> GenerateQuizAsync(int userId, int count = 10)
    {
        // 1) Öğrenilmemiş bütün kelimeleri çek (sırasız)
        var pool = await _db.Words
            .Where(w => w.OwnerId == userId)
            .Where(w => !_db.UserWordProgresses
                         .Any(p => p.UserID == userId &&
                                   p.WordID == w.WordID &&
                                   p.IsLearned))          // sadece öğrenilmemiş
            .Include(w => w.Samples)
            .ToListAsync();

        // 2) Fisher–Yates karışımı ve ilk 'count' adet seç
        var rng = new Random();
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        var pick = pool.Take(count);

        // 3) DTO listesi üret
        var list = new List<QuizQuestionDto>();
        foreach (var w in pick)
        {
            var qType = PickQuestionType(w);
            list.Add(new QuizQuestionDto
            {
                WordID = w.WordID,
                EngWord = w.EngWordName,
                TurkWord = w.TurWordName,
                PicturePath = w.Picture,
                SentenceHtml = BuildSentenceHtml(w),
                QuestionType = qType
            });
        }
        return list;
    }

    // ---------- Cevabı işle ----------
    public async Task RecordAnswerAsync(int userId, int wordId, bool isCorrect, QuestionType qType)
    {
        // log
        _db.QuestionAttempts.Add(new QuestionAttempt
        {
            UserID = userId,
            WordID = wordId,
            QuestionType = qType,
            IsCorrect = isCorrect,
            AskedAtUtc = DateTime.UtcNow
        });

        var progress = await _db.UserWordProgresses
            .FirstOrDefaultAsync(p => p.UserID == userId && p.WordID == wordId);

        if (progress == null)
        {
            progress = new UserWordProgress
            {
                UserID = userId,
                WordID = wordId,
                CorrectStreak = 0,
                IsLearned = false
            };
            _db.UserWordProgresses.Add(progress);
        }

        if (progress.IsLearned)
        {
            await _db.SaveChangesAsync();
            return;                     // zaten bitmiş
        }

        if (isCorrect)
        {
            progress.CorrectStreak++;
            if (progress.CorrectStreak >= 6)
                progress.IsLearned = true;
        }
        else
        {
            progress.CorrectStreak = 0; // seri bozuldu
        }

        await _db.SaveChangesAsync();
    }

    // ---------- Yardımcılar ----------
    private static readonly Random _rng = new();

    private static QuestionType PickQuestionType(Word w)
    {
        // Uygun tipleri topla
        var possible = new List<QuestionType> { QuestionType.EngToTurk };   // her kelime text’le sorulabilir

        if (!string.IsNullOrWhiteSpace(w.Picture))
            possible.Add(QuestionType.PictureToEng);

        if (w.Samples.Any())
            possible.Add(QuestionType.SentenceToTurk);

        // Ağırlık örneği: Text %40, Picture %30, Sentence %30
        var weights = new Dictionary<QuestionType, int>
        {
            [QuestionType.EngToTurk] = 40,
            [QuestionType.PictureToEng] = 30,
            [QuestionType.SentenceToTurk] = 30
        };

        // Sadece mümkün tiplerin ağırlıklarını topla
        var pool = possible.SelectMany(t => Enumerable.Repeat(t, weights[t])).ToList();

        return pool[_rng.Next(pool.Count)];
    }

    private static string BuildSentenceHtml(Word w)
    {
        var sample = w.Samples.FirstOrDefault()?.Samples ?? $"I have an {w.EngWordName}.";
        // kelimeyi altı çizili yap
        return sample.Replace(w.EngWordName,
               $"<u>{w.EngWordName}</u>", StringComparison.OrdinalIgnoreCase);
    }
}

// ---------- DTO ----------
public class QuizQuestionDto
{
    public int WordID { get; set; }
    public QuestionType QuestionType { get; set; }

    public string? EngWord { get; set; }   // EngToTurk sorusu
    public string? TurkWord { get; set; }   // kontrol için
    public string? PicturePath { get; set; }// PictureToEng
    public string? SentenceHtml { get; set; }// SentenceToTurk
}
