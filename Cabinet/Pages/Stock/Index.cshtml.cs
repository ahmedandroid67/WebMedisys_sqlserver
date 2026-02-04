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
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Cabinet.Models.Stock> StockList { get; set; } = new();
        public List<CategoryStock> Categories { get; set; } = new();

        // List to populate the employee dropdown in the movement modal
        public List<Employer> EmployeeOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryFilter { get; set; }

        [BindProperty]
        public CategoryStock NewCategory { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Load categories and employees for modals and filters
            Categories = await _context.CategoryStocks.ToListAsync();
            EmployeeOptions = await _context.Employer.OrderBy(e => e.Nom).ToListAsync();

            var query = _context.Stocks
                .Include(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
                query = query.Where(s => s.Nom.Contains(SearchString));

            if (CategoryFilter.HasValue)
                query = query.Where(s => s.CategoryId == CategoryFilter);

            StockList = await query.ToListAsync();
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

        // UNIFIED Handler with Safety Checks
        public async Task<IActionResult> OnPostRecordMovementAsync(int MoveStockId, int MoveQty, string MoveType, string MoveMotif, DateTime MoveDate, int MoveEmployerId)
        {
            var product = await _context.Stocks.FindAsync(MoveStockId);
            if (product == null) return RedirectToPage();

            // Safety Check: Validate quantity for Sortie (Exits)
            if (MoveType == "Sortie" && MoveQty > product.Quantite)
            {
                TempData["ErrorMessage"] = $"Erreur: Vous essayez de sortir {MoveQty} unités, mais il n'en reste que {product.Quantite} en stock.";
                return RedirectToPage();
            }

            // 1. Create the Movement record with traceability data
            var movement = new StockMovement
            {
                StockId = MoveStockId,
                Quantite = (MoveType == "Entrée") ? MoveQty : -MoveQty,
                Type = MoveType,
                Motif = MoveMotif,
                DateMouvement = MoveDate,
                EmployerId = MoveEmployerId
            };

            // 2. Update current stock level
            product.Quantite += movement.Quantite;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // Legacy support method for simple +/- buttons if still used
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