using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Cabinet.Pages.Rendezvous
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cabinet.Models.Rendezvous> RendezvousList { get; set; } = new List<Cabinet.Models.Rendezvous>();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        // Added DateFilter property
        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DateFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Rendezvous.AsQueryable();

            // 1. Filter by Name/Phone Search String
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                query = query.Where(r =>
                    (r.Nom != null && r.Nom.Contains(SearchString)) ||
                    (r.Prenom != null && r.Prenom.Contains(SearchString)) ||
                    (r.Phone != null && r.Phone.Contains(SearchString))
                );
            }

            // 2. Filter by Specific Date
            if (DateFilter.HasValue)
            {
                query = query.Where(r => r.DateHeure.Date == DateFilter.Value.Date);
            }

            RendezvousList = await query
                .OrderByDescending(r => r.DateHeure)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var rendezvous = await _context.Rendezvous.FindAsync(id);

            if (rendezvous != null)
            {
                _context.Rendezvous.Remove(rendezvous);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}