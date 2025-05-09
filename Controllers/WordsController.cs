using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordMemoryApp.Data;
using WordMemoryApp.Models;

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

        // Kelime listesi
        public async Task<IActionResult> Index()
        {
            var words = await _context.Words
                .Include(w => w.Samples)
                .ToListAsync();
            return View(words);
        }

        // GET: Kelime ekleme formu
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kelime ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string EngWordName,
            string TurWordName,
            string samples,
            IFormFile? PictureFile)
        {
            if (!ModelState.IsValid)
                return View();

            var word = new Word
            {
                EngWordName = EngWordName,
                TurWordName = TurWordName
            };

            // Resim kaydet
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

            _context.Words.Add(word);
            await _context.SaveChangesAsync();

            // Örnek cümleleri satır satır ekle
            var lines = samples
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                _context.WordSamples.Add(new WordSample
                {
                    WordID = word.WordID,
                    Samples = line
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: Words/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var word = await _context.Words
                                     .Include(w => w.Samples)
                                     .FirstOrDefaultAsync(w => w.WordID == id);
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
            // 1) Var mı kontrol
            var word = await _context.Words
                                     .Include(w => w.Samples)
                                     .FirstOrDefaultAsync(w => w.WordID == WordID);
            if (word == null) return NotFound();

            // 2) Temel alanları güncelle
            word.EngWordName = EngWordName;
            word.TurWordName = TurWordName;

            // 3) Resim güncelleme
            if (PictureFile != null && PictureFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "words");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(PictureFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await PictureFile.CopyToAsync(stream);

                // (İsteğe bağlı) eski resmi sil
                if (!string.IsNullOrEmpty(word.Picture))
                {
                    var oldFullPath = Path.Combine(_env.WebRootPath, word.Picture);
                    if (System.IO.File.Exists(oldFullPath))
                        System.IO.File.Delete(oldFullPath);
                }

                word.Picture = Path.Combine("images/words", fileName).Replace("\\", "/");
            }

            // 4) Örnek cümleleri yenile
            _context.WordSamples.RemoveRange(word.Samples);
            if (!string.IsNullOrWhiteSpace(samples))
            {
                var lines = samples
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(text => text.Trim());

                foreach (var line in lines)
                {
                    word.Samples.Add(new WordSample
                    {
                        WordID = word.WordID,
                        Samples = line
                    });
                }
            }

            // 5) Kaydet ve yönlendir
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

        // 4) POST: /Words/DeleteConfirmed/5  -----------------------------
        [HttpPost, ActionName("DeleteConfirmed"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var word = await _context.Words.FindAsync(id);
            if (word is not null)
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
