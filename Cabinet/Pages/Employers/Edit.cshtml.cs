using Cabinet.Data;
using Cabinet.Models;
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Tell the database that this specific Employer has been modified
            _context.Attach(Employer).State = EntityState.Modified;

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