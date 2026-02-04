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
                _context.Employer.Remove(employer);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}