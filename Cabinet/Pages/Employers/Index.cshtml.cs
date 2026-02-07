using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Employers
{
    [Authorize] // Protect this page
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Employer> Employers { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Employer != null)
            {
                Employers = await _context.Employer.ToListAsync();
            }
        }

        // Delete Handler
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var employer = await _context.Employer.FindAsync(id);

            if (employer != null)
            {
                var currentUserEmail = User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(currentUserEmail) &&
                    string.Equals(employer.Email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Suppression impossible: vous ne pouvez pas supprimer votre propre compte.";
                    return RedirectToPage("./Index");
                }

                if (string.Equals(employer.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminCount = await _context.Employer.CountAsync(e => e.Role == "Admin");
                    if (adminCount <= 1)
                    {
                        TempData["ErrorMessage"] = "Suppression impossible: au moins un compte Admin doit rester actif.";
                        return RedirectToPage("./Index");
                    }
                }

                _context.Employer.Remove(employer);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
