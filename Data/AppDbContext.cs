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
              //soru modulu
              public DbSet<UserWordProgress> UserWordProgresses { get; set; } = null!;
              public DbSet<QuestionAttempt> QuestionAttempts { get; set; } = null!;
              //setting
              public DbSet<UserSettings> UserSettings { get; set; } = null!;
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

                     // QuestionType enum'u tinyint sakla
                     builder.Entity<QuestionAttempt>()
                            .Property(q => q.QuestionType)
                            .HasConversion<byte>();

                     // User + Word ikilisi benzersiz
                     builder.Entity<UserWordProgress>()
                            .HasIndex(p => new { p.UserID, p.WordID })
                            .IsUnique();

                     /* FK: UserWordProgress → User  (silinirse ilerlemeler de silinsin) */
                     builder.Entity<UserWordProgress>()
                            .HasOne(p => p.User)
                            .WithMany()
                            .HasForeignKey(p => p.UserID)
                            .OnDelete(DeleteBehavior.Cascade);

                     /* FK: UserWordProgress → Word  (silme KISITLI – cascade yok) */
                     builder.Entity<UserWordProgress>()
                            .HasOne(p => p.Word)
                            .WithMany()
                            .HasForeignKey(p => p.WordID)
                            .OnDelete(DeleteBehavior.Restrict);

                     /* FK: QuestionAttempt → User  (silme KISITLI) */
                     builder.Entity<QuestionAttempt>()
                            .HasOne(a => a.User)
                            .WithMany()
                            .HasForeignKey(a => a.UserID)
                            .OnDelete(DeleteBehavior.Restrict);

                     /* FK: QuestionAttempt → Word  (silme KISITLI – cascade yok) */
                     builder.Entity<QuestionAttempt>()
                            .HasOne(a => a.Word)
                            .WithMany()
                            .HasForeignKey(a => a.WordID)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.Entity<UserSettings>()
                            .HasOne(s => s.User)
                            .WithOne()
                            .HasForeignKey<UserSettings>(s => s.UserID)
                            .OnDelete(DeleteBehavior.Cascade);

              }
       }
}
