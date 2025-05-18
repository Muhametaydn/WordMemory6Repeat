using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordMemoryApp.Data;
using WordMemoryApp.Models;

namespace WordMemoryApp.Helpers
{
    public static class WordleHelper
    {
        /// <summary>Kullanıcının “IsLearned=true” kelimelerinden rastgele birini getirir.</summary>
        public static async Task<Word?> GetRandomLearnedWordAsync(AppDbContext db, int userId)
        {
            return await db.Words
                           .Where(w => db.UserWordProgresses
                               .Any(p => p.UserID == userId &&
                                         p.WordID == w.WordID &&
                                         p.IsLearned))                    // ↲ kesin                        
                           .OrderBy(_ => Guid.NewGuid())                  // rastgele
                           .FirstOrDefaultAsync();
        }
    }
}
