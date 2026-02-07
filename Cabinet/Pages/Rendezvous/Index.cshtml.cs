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
        private const int PageSize = 25;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Models.Rendezvous> RendezvousList { get; set; } = new List<Models.Rendezvous>();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Rendezvous.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                query = query.Where(r =>
                    (r.Nom != null && r.Nom.Contains(SearchString)) ||
                    (r.Prenom != null && r.Prenom.Contains(SearchString)) ||
                    (r.Phone != null && r.Phone.Contains(SearchString))
                );
            }

            if (DateFilter.HasValue)
            {
                query = query.Where(r => r.DateHeure.Date == DateFilter.Value.Date);
            }

            query = query.OrderByDescending(r => r.DateHeure);

            TotalCount = await query.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            PageNumber = Math.Min(Math.Max(1, PageNumber), TotalPages);

            RendezvousList = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
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
