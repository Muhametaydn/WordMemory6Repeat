using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMemoryApp.Data;
using WordMemoryApp.Extensions;
using WordMemoryApp.Helpers;
using WordMemoryApp.Models;

namespace WordMemoryApp.Controllers
{
    [Authorize]
    [Route("Puzzle")]
    public class PuzzleController : Controller
    {
        private readonly AppDbContext _db;
        public PuzzleController(AppDbContext db) => _db = db;

        // GET /Puzzle
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {

            var state = HttpContext.Session.LoadGame();

            if (state == null || state.IsFinished)
            {
                int userId = int.Parse(User.FindFirst("UserID")!.Value);
                var word = await WordleHelper.GetRandomLearnedWordAsync(_db, userId);
                if (word == null) return View("NoWords");

                state = new PuzzleState(word.EngWordName.ToLowerInvariant(), 0, false);
                HttpContext.Session.SaveGame(state);
            }
            ViewBag.TargetWord = state.Target;   // Index GET’inde

            ViewBag.Len = state.Target.Length;          // sütun sayısı
            ViewBag.MaxAttempt = PuzzleState.MaxAttempts;      // her zaman 5 satır
            return View(state);
        }

        // POST /Puzzle/Guess
        [HttpPost("Guess")]
        public IActionResult Guess([FromForm] string attempt)
        {
            var state = HttpContext.Session.LoadGame();
            if (state == null || state.IsFinished)
                return BadRequest("Oyun yok");

            attempt = attempt.Trim().ToLowerInvariant();
            if (attempt.Length != state.Target.Length)
                return BadRequest($"Kelime {state.Target.Length} harf olmalı");

            string mask = Compare(attempt, state.Target);
            bool correct = attempt == state.Target;
            bool finished = correct || state.Attempts + 1 >= PuzzleState.MaxAttempts;

            state = state with { Attempts = state.Attempts + 1, IsFinished = finished };
            HttpContext.Session.SaveGame(state);

            return Json(new
            {
                mask,
                row = state.Attempts - 1,   // boyanacak satır
                correct,
                finished,
                answer = state.Target          // bittiğinde göster
            });
        }

        /* --------------------------------------------------------- */
        /* == Yardımcı == */
        private static string Compare(string guess, string target)
        {
            var res = new char[guess.Length];
            var ta = target.ToCharArray();

            // 1) doğru yer
            for (int i = 0; i < guess.Length; i++)
                if (guess[i] == ta[i]) { res[i] = 'g'; ta[i] = '*'; }

            // 2) yanlış yerde/hiç yok
            for (int i = 0; i < guess.Length; i++)
                if (res[i] == '\0')
                {
                    int idx = Array.IndexOf(ta, guess[i]);
                    res[i] = idx >= 0 ? 'y' : 'b';
                    if (idx >= 0) ta[idx] = '*';
                }
            return new string(res);
        }
        // GET /Puzzle/New
        [HttpGet("New")]
        public IActionResult New()
        {
            HttpContext.Session.ClearGame();      // önceki oyunu sil
            return RedirectToAction(nameof(Index)); // sonra Index'e yönlendir
        }

    }
}
