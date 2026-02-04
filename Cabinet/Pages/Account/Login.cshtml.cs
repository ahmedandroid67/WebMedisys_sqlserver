using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims; // CRITICAL: Fixes the 'BinaryReader' error
using Cabinet.Data;
using Cabinet.Models;

namespace Cabinet.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
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

            // 1. Search for employer using the property 'MotPasse' from your model
            var user = await _context.Employer
                .FirstOrDefaultAsync(u => u.Email == Input.Email && u.MotPasse == Input.Password);

            if (user != null)
            {
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

            // If login fails
            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            return Page();
        }
    }
}