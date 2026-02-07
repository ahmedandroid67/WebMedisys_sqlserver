using Cabinet.Data;
using Cabinet.Models;
using Cabinet.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // <--- This is required for database commands

namespace Cabinet.Pages.Employers
{
    [Authorize]
    public class EditModel : PageModel // <--- Class name must be 'EditModel'
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employer Employer { get; set; } = default!;

        // 1. GET: Fetches the data to fill the form
        // The parameter name 'id' here must match the URL ?id=3
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Make sure you are searching by IdEmployer
            var employer = await _context.Employer.FirstOrDefaultAsync(m => m.IdEmployer == id);

            if (employer == null) return NotFound();

            Employer = employer; // This is the line that fills the form
            return Page();
        }

        // 2. POST: Saves the changes
        public async Task<IActionResult> OnPostAsync()
        {
            var changingPassword = !string.IsNullOrWhiteSpace(Employer.MotPasse);
            if (!changingPassword)
            {
                // Password update is optional on this form.
                ModelState.Remove("Employer.MotPasse");
                ModelState.Remove("Employer.ConfirmMotPasse");
            }
            else if (!string.Equals(Employer.MotPasse, Employer.ConfirmMotPasse, StringComparison.Ordinal))
            {
                ModelState.AddModelError("Employer.ConfirmMotPasse", "Les mots de passe ne correspondent pas.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _context.Employer.FirstOrDefaultAsync(e => e.IdEmployer == Employer.IdEmployer);
            if (existing == null)
            {
                return NotFound();
            }

            var normalizedEmail = Employer.Email.Trim().ToLowerInvariant();
            var duplicateEmail = await _context.Employer
                .AnyAsync(e => e.IdEmployer != Employer.IdEmployer && e.Email != null && e.Email.ToLower() == normalizedEmail);
            if (duplicateEmail)
            {
                ModelState.AddModelError("Employer.Email", "Cet email existe déjà.");
                return Page();
            }

            existing.Nom = Employer.Nom;
            existing.Prenom = Employer.Prenom;
            existing.Email = normalizedEmail;
            existing.Role = Employer.Role;
            existing.Fonction = Employer.Fonction;
            existing.Telephone = Employer.Telephone;
            existing.Adresse = Employer.Adresse;

            if (changingPassword)
            {
                existing.MotPasse = PasswordSecurity.HashPassword(Employer.MotPasse);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployerExists(Employer.IdEmployer))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        // Helper function to check if ID exists
        private bool EmployerExists(int id)
        {
            return _context.Employer.Any(e => e.IdEmployer == id);
        }
    }
}
