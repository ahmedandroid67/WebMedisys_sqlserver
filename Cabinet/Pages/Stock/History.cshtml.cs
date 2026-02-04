using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Stock
{
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public HistoryModel(ApplicationDbContext context) => _context = context;

        public List<StockMovement> Movements { get; set; } = new();
        public List<SelectListItem> ProductOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ProductFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Load products for the filter dropdown
            ProductOptions = await _context.Stocks
                .OrderBy(s => s.Nom)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Nom })
                .ToListAsync();

            // 2. Build the query
            var query = _context.StockMovements
                .Include(m => m.Stock)
                .Include(m => m.Employer)
                .AsQueryable();

            // 3. Apply Filters
            if (ProductFilter.HasValue)
                query = query.Where(m => m.StockId == ProductFilter);

            if (!string.IsNullOrEmpty(TypeFilter))
                query = query.Where(m => m.Type == TypeFilter);

            Movements = await query.OrderByDescending(m => m.DateMouvement).ToListAsync();
        }
    }
}