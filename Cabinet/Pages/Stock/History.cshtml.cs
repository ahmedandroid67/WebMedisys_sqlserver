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
        private const int PageSize = 25;

        public HistoryModel(ApplicationDbContext context) => _context = context;

        public List<StockMovement> Movements { get; set; } = new();
        public List<SelectListItem> ProductOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ProductFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            ProductOptions = await _context.Stocks
                .AsNoTracking()
                .OrderBy(s => s.Nom)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Nom })
                .ToListAsync();

            var query = _context.StockMovements
                .AsNoTracking()
                .Include(m => m.Stock)
                .Include(m => m.Employer)
                .AsQueryable();

            if (ProductFilter.HasValue)
                query = query.Where(m => m.StockId == ProductFilter.Value);

            if (!string.IsNullOrEmpty(TypeFilter))
                query = query.Where(m => m.Type == TypeFilter);

            query = query.OrderByDescending(m => m.DateMouvement);

            TotalCount = await query.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            PageNumber = Math.Min(Math.Max(1, PageNumber), TotalPages);

            Movements = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
