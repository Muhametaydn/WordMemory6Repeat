using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordMemoryApp.Data;
using WordMemoryApp.Models;
using System.Security.Claims;
namespace WordMemoryApp.Controllers
{
    [Authorize]
    public class WordsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public WordsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        // Yardımcı: şu anki kullanıcının UserID'sini getir
        private async Task<int?> GetCurrentUserIdAsync()
        {
            // 1) Eğer login sonrası ClaimTypes.NameIdentifier'a atıyorsanız:
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idClaim, out var uid))
                return uid;

            // 2) Ya da kullanıcı adını Name üzerinden alıp DB'den çekebilirsiniz:
            var username = User.Identity?.Name;
            if (username != null)
            {
                var u = await _context.Users
                             .FirstOrDefaultAsync(x => x.UserName == username);
                return u?.UserID;
            }

            return null;
        }
        // Kelime listesi
        // GET: Words
        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null) return Forbid();

            var words = await _context.Words
                .Where(w => w.OwnerId == userId.Value)
                .Include(w => w.Samples)
                .ToListAsync();
            return View(words);
        }

        // GET: Kelime ekleme formu
        // GET: Words/Create
        public IActionResult Create() => View();

        // POST: Words/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string EngWordName,
            string TurWordName,
            string samples,
            IFormFile? PictureFile)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null) return Forbid();

            if (!ModelState.IsValid) return View();

            var word = new Word
            {
                EngWordName = EngWordName,
                TurWordName = TurWordName,
                OwnerId = userId.Value
            };

            // — Resim kaydet
            if (PictureFile != null && PictureFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "words");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(PictureFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await PictureFile.CopyToAsync(stream);
                word.Picture = Path.Combine("images/words", fileName).Replace("\\", "/");
            }

            // — Örnek cümleleri ekle
            if (!string.IsNullOrWhiteSpace(samples))
            {
                var lines = samples
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim());

                foreach (var line in lines)
                    word.Samples.Add(new WordSample { Samples = line });
            }

            _context.Words.Add(word);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        // GET: Words/Edit/5
        // GET: Words/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null) return Forbid();

            var word = await _context.Words
                            .Include(w => w.Samples)
                            .FirstOrDefaultAsync(w => w.WordID == id && w.OwnerId == userId);
            if (word == null) return NotFound();

            return View(word);
        }

        // POST: Words/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int WordID,
            string EngWordName,
            string TurWordName,
            string samples,
            IFormFile? PictureFile)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null) return Forbid();

            var word = await _context.Words
                            .Include(w => w.Samples)
                            .FirstOrDefaultAsync(w => w.WordID == WordID && w.OwnerId == userId);
            if (word == null) return NotFound();

            word.EngWordName = EngWordName;
            word.TurWordName = TurWordName;

            // — Resim güncelle
            if (PictureFile != null && PictureFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "words");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid() + Path.GetExtension(PictureFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await PictureFile.CopyToAsync(stream);
                if (!string.IsNullOrEmpty(word.Picture))
                {
                    var old = Path.Combine(_env.WebRootPath, word.Picture);
                    if (System.IO.File.Exists(old))
                        System.IO.File.Delete(old);
                }
                word.Picture = Path.Combine("images/words", fileName).Replace("\\", "/");
            }

            // — Örnek cümleleri yenile
            _context.WordSamples.RemoveRange(word.Samples);
            if (!string.IsNullOrWhiteSpace(samples))
            {
                var lines = samples
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim());
                foreach (var line in lines)
                    word.Samples.Add(new WordSample { Samples = line });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }





        // 3) GET: /Words/Delete/5  (isteğe bağlı: “emin misin?” sayfası) --
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var word = await _context.Words.FindAsync(id);
            if (word is null) return NotFound();
            return View(word);            // Views/Words/Delete.cshtml
        }

        // POST: Words/DeleteConfirmed/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null) return Forbid();

            var word = await _context.Words
                            .FirstOrDefaultAsync(w => w.WordID == id && w.OwnerId == userId);
            if (word != null)
            {
                _context.Words.Remove(word);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WordExists(int id)
           => _context.Words.Any(e => e.WordID == id);
    }
}
