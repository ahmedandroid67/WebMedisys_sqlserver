using Cabinet.Data;
using Cabinet.Models;
using Cabinet.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Employers
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Employer Employer { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var email = Employer.Email.Trim().ToLowerInvariant();
            var emailExists = await _context.Employer.AnyAsync(e => e.Email != null && e.Email.ToLower() == email);
            if (emailExists)
            {
                ModelState.AddModelError("Employer.Email", "Cet email existe déjà.");
                return Page();
            }

            Employer.Email = email;
            Employer.MotPasse = PasswordSecurity.HashPassword(Employer.MotPasse);

            _context.Employer.Add(Employer);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
