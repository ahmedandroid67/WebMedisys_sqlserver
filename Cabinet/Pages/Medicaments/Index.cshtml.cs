using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Medicaments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Medicament> Medicaments { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Medicament.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(m =>
                    (m.Nom != null && m.Nom.Contains(SearchString)) ||
                    (m.Code != null && m.Code.Contains(SearchString)) ||
                    (m.Dci1 != null && m.Dci1.Contains(SearchString)));
            }

            // Limit to 50 results to prevent crashing the page if the list is huge
            Medicaments = await query.OrderBy(m => m.Nom).Take(50).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(double? id)
        {
            if (id == null) return NotFound();

            var medicament = await _context.Medicament.FindAsync(id);

            if (medicament != null)
            {
                _context.Medicament.Remove(medicament);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}