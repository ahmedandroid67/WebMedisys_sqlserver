using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cabinet.Data;
using Cabinet.Models;

namespace Cabinet.Pages.Stock
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 25;

        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Models.Stock> StockList { get; set; } = new();
        public List<CategoryStock> Categories { get; set; } = new();
        public List<Employer> EmployeeOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty]
        public CategoryStock NewCategory { get; set; } = new();

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.CategoryStocks
                .AsNoTracking()
                .OrderBy(c => c.Nom)
                .ToListAsync();

            EmployeeOptions = await _context.Employer
                .AsNoTracking()
                .OrderBy(e => e.Nom)
                .ToListAsync();

            var query = _context.Stocks
                .AsNoTracking()
                .Include(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                query = query.Where(s => s.Nom.Contains(SearchString));
            }

            if (CategoryFilter.HasValue)
            {
                query = query.Where(s => s.CategoryId == CategoryFilter.Value);
            }

            query = query.OrderBy(s => s.Nom);

            TotalCount = await query.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            PageNumber = Math.Min(Math.Max(1, PageNumber), TotalPages);

            StockList = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategory.Nom))
            {
                TempData["ErrorMessage"] = "Le nom de la catégorie ne peut pas être vide.";
                return RedirectToPage();
            }

            _context.CategoryStocks.Add(NewCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteProductAsync(int id)
        {
            var product = await _context.Stocks.FindAsync(id);
            if (product != null)
            {
                _context.Stocks.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
        {
            var hasProducts = await _context.Stocks.AnyAsync(s => s.CategoryId == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Impossible de supprimer : cette catégorie contient des produits.";
                return RedirectToPage();
            }

            var cat = await _context.CategoryStocks.FindAsync(id);
            if (cat != null)
            {
                _context.CategoryStocks.Remove(cat);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRecordMovementAsync(int MoveStockId, int MoveQty, string MoveType, string MoveMotif, DateTime MoveDate, int MoveEmployerId)
        {
            var product = await _context.Stocks.FindAsync(MoveStockId);
            if (product == null) return RedirectToPage();

            if (MoveType == "Sortie" && MoveQty > product.Quantite)
            {
                TempData["ErrorMessage"] = $"Erreur: Vous essayez de sortir {MoveQty} unités, mais il n'en reste que {product.Quantite} en stock.";
                return RedirectToPage();
            }

            var movement = new StockMovement
            {
                StockId = MoveStockId,
                Quantite = (MoveType == "Entrée") ? MoveQty : -MoveQty,
                Type = MoveType,
                Motif = MoveMotif,
                DateMouvement = MoveDate,
                EmployerId = MoveEmployerId
            };

            product.Quantite += movement.Quantite;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStockAsync(int id, int amount, string type)
        {
            var product = await _context.Stocks.FindAsync(id);
            if (product != null)
            {
                if (type == "in") product.Quantite += amount;
                else if (type == "out") product.Quantite = Math.Max(0, product.Quantite - amount);

                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
