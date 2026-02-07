using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims; // CRITICAL: Fixes the 'BinaryReader' error
using Microsoft.Extensions.Caching.Memory;
using Cabinet.Data;
using Cabinet.Models;
using Cabinet.Security;

namespace Cabinet.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LoginModel> _logger;
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public LoginModel(ApplicationDbContext context, IMemoryCache cache, ILogger<LoginModel> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "L'email est requis")]
            [EmailAddress(ErrorMessage = "Format d'email invalide")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Le mot de passe est requis")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var email = Input.Email.Trim().ToLowerInvariant();
            if (IsLockedOut(email, out var lockedUntilUtc))
            {
                var remaining = lockedUntilUtc - DateTime.UtcNow;
                var remainingMinutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
                ModelState.AddModelError(string.Empty, $"Compte temporairement bloqué. Réessayez dans {remainingMinutes} minute(s).");
                return Page();
            }

            var user = await _context.Employer
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == email);

            if (user == null)
            {
                RegisterFailedAttempt(email);
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return Page();
            }

            var isValidPassword = PasswordSecurity.VerifyPassword(user.MotPasse, Input.Password, out var needsRehash);
            if (!isValidPassword)
            {
                RegisterFailedAttempt(email);
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return Page();
            }

            if (needsRehash)
            {
                user.MotPasse = PasswordSecurity.HashPassword(Input.Password);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Legacy password upgraded to hashed for user {Email}.", user.Email);
            }

            ClearFailedAttempts(email);

            // 2. Setup the identity claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                // This creates the "FullName" claim used in your _Layout.cshtml
                new Claim("FullName", $"{user.Nom} {user.Prenom}"),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Sign in the user to create the encrypted cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true // Keeps the session active after closing the browser
                });

            return RedirectToPage("/Index");
        }

        private static string FailedAttemptsKey(string email) => $"auth:failed:{email}";
        private static string LockoutKey(string email) => $"auth:lock:{email}";

        private bool IsLockedOut(string email, out DateTime lockedUntilUtc)
        {
            if (_cache.TryGetValue(LockoutKey(email), out DateTime lockUntil) && lockUntil > DateTime.UtcNow)
            {
                lockedUntilUtc = lockUntil;
                return true;
            }

            lockedUntilUtc = DateTime.MinValue;
            return false;
        }

        private void RegisterFailedAttempt(string email)
        {
            var attempts = _cache.TryGetValue(FailedAttemptsKey(email), out int existing) ? existing + 1 : 1;
            _cache.Set(FailedAttemptsKey(email), attempts, LockoutDuration);

            if (attempts >= MaxFailedAttempts)
            {
                var lockUntil = DateTime.UtcNow.Add(LockoutDuration);
                _cache.Set(LockoutKey(email), lockUntil, LockoutDuration);
                _cache.Remove(FailedAttemptsKey(email));
                _logger.LogWarning("User {Email} locked out until {LockUntilUtc}.", email, lockUntil);
            }
        }

        private void ClearFailedAttempts(string email)
        {
            _cache.Remove(FailedAttemptsKey(email));
            _cache.Remove(LockoutKey(email));
        }
    }
}
