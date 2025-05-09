using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordMemoryApp.Data;
using WordMemoryApp.Models;
using Microsoft.AspNetCore.Authorization;
namespace WordMemoryApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public AccountController(AppDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // ----- KAYIT -----
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register(string userName, string email, string password)
        {
            if (!ModelState.IsValid)
                return View();

            if (_context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Bu e-posta zaten kayıtlı");
                return View();
            }
            if (_context.Users.Any(u => u.UserName == userName))
            {
                ModelState.AddModelError("", "Bu kullanıcı adı alınmış");
                return View();
            }

            var user = new User { UserName = userName, Email = email };
            user.PasswordHash = _hasher.HashPassword(user, password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Login));
        }


        // ----- GİRİŞ -----
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
                    != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("", "Hatalı e-posta veya şifre");
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),     // ← kullanıcı adı
        new Claim(ClaimTypes.Email, user.Email),       // ← e-posta ihtiyacın olursa
        new Claim("UserId", user.UserID.ToString())
    };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            return RedirectToAction("Index", "Home");
        }

        // ----- ÇIKIŞ -----
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ----- Yetki hatası -----
        public IActionResult AccessDenied() => View();


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            var prt = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expiration > DateTime.UtcNow);

            if (prt == null)
            {
                TempData["Error"] = "Geçersiz veya süresi dolmuş bağlantı.";
                return RedirectToAction("ForgotPassword");
            }

            return View(model: token);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var prt = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expiration > DateTime.UtcNow);

            if (prt == null)
            {
                TempData["Error"] = "Bağlantı geçersiz veya süresi dolmuş.";
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.Users.FindAsync(prt.UserID);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("ForgotPassword");
            }

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            _context.PasswordResetTokens.Remove(prt);  // token'ı iptal et
            await _context.SaveChangesAsync();

            TempData["Success"] = "Şifreniz başarıyla güncellendi.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }



        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Kullanıcı yoksa bile link gönderildi mesajı gösteririz (güvenlik için)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Token üret
            var token = Guid.NewGuid().ToString("N");
            var resetToken = new PasswordResetToken
            {
                UserID = user.UserID,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1)
            };
            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Link üret
            var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);

            // E-posta gönderimi (log’a da yazabilirsin)
            Console.WriteLine($"Şifre sıfırlama bağlantısı: {resetLink}");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }



    }


}
