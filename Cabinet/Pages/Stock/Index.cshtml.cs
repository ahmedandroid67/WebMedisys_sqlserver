using System.Security.Claims;
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

            // Bug 10 fix: prevent duplicate category names
            var exists = await _context.CategoryStocks
                .AnyAsync(c => c.Nom == NewCategory.Nom.Trim());
            if (exists)
            {
                TempData["ErrorMessage"] = $"La catégorie '{NewCategory.Nom}' existe déjà.";
                return RedirectToPage();
            }

            NewCategory.Nom = NewCategory.Nom.Trim();
            _context.CategoryStocks.Add(NewCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteProductAsync(int id)
        {
            var hasMovements = await _context.StockMovements.AnyAsync(m => m.StockId == id);
            if (hasMovements)
            {
                TempData["ErrorMessage"] = "Impossible de supprimer : ce produit a un historique de mouvements. Supprimez d'abord les mouvements.";
                return RedirectToPage();
            }

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

        public async Task<IActionResult> OnPostRecordMovementAsync(int MoveStockId, int MoveQty, string MoveType, string MoveMotif, DateTime MoveDate)
        {
            if (MoveType != "Entrée" && MoveType != "Sortie")
            {
                TempData["ErrorMessage"] = "Type de mouvement invalide. Utilisez 'Entrée' ou 'Sortie'.";
                return RedirectToPage();
            }

            if (MoveQty <= 0)
            {
                TempData["ErrorMessage"] = "La quantité doit être un nombre positif.";
                return RedirectToPage();
            }

            var employerIdClaim = User.FindFirstValue("EmployerId");
            if (string.IsNullOrEmpty(employerIdClaim) || !int.TryParse(employerIdClaim, out var employerId))
            {
                TempData["ErrorMessage"] = "Impossible d'identifier l'employé connecté.";
                return RedirectToPage();
            }

            var product = await _context.Stocks.FindAsync(MoveStockId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Produit introuvable.";
                return RedirectToPage();
            }

            // Atomic stock update with overflow guard
            if (MoveType == "Sortie")
            {
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE stock SET quantite = quantite - {0}, updated_at = GETUTCDATE() WHERE id_produit = {1} AND quantite >= {0}",
                    MoveQty, MoveStockId);

                if (rows == 0)
                {
                    var currentQty = await _context.Stocks.AsNoTracking()
                        .Where(s => s.Id == MoveStockId)
                        .Select(s => s.Quantite)
                        .FirstOrDefaultAsync();

                    TempData["ErrorMessage"] = $"Erreur: Vous essayez de sortir {MoveQty} unités, mais il n'en reste que {currentQty} en stock.";
                    return RedirectToPage();
                }
            }
            else
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE stock SET quantite = quantite + {0}, updated_at = GETUTCDATE() WHERE id_produit = {1}",
                    MoveQty, MoveStockId);
            }

            var movement = new StockMovement
            {
                StockId = MoveStockId,
                Quantite = (MoveType == "Entrée") ? MoveQty : -MoveQty,
                Type = MoveType,
                Motif = MoveMotif,
                DateMouvement = MoveDate,
                EmployerId = employerId
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

    }
}
