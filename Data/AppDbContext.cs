using Microsoft.EntityFrameworkCore;
using WordMemoryApp.Models;

namespace WordMemoryApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Kullanıcılar tablosu
        public DbSet<User> Users { get; set; } = null!;

        // Şifre sıfırlama tokenları
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        public DbSet<Word> Words { get; set; } = null!;
        public DbSet<WordSample> WordSamples { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Email unique index
            builder.Entity<User>()
                   .HasIndex(u => u.Email)
                   .IsUnique();

            // PasswordResetToken → User ilişkisi
            builder.Entity<PasswordResetToken>()
                   .HasOne(prt => prt.User)
                   .WithMany()
                   .HasForeignKey(prt => prt.UserID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Word>()
                    .HasOne(w => w.Owner)
                    .WithMany(u => u.Words)
                    .HasForeignKey(w => w.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
