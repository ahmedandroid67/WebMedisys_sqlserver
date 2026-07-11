using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cabinet.Data;
using Cabinet.Models;

namespace Cabinet.Pages.Stock
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // BindProperty allows the form data to be automatically mapped to this object
        [BindProperty]
        public Cabinet.Models.Stock NewStock { get; set; } = new();

        // This list will populate the category dropdown in the HTML
        public List<CategoryStock> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Load categories to display in the dropdown
            CategoryOptions = await _context.CategoryStocks.OrderBy(c => c.Nom).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove validation for the navigation property to prevent model state errors
            ModelState.Remove("NewStock.Category");

            if (!ModelState.IsValid)
            {
                CategoryOptions = await _context.CategoryStocks.OrderBy(c => c.Nom).ToListAsync();
                return Page();
            }

            var catExists = await _context.CategoryStocks.AnyAsync(c => c.Id == NewStock.CategoryId);
            if (!catExists)
            {
                ModelState.AddModelError("NewStock.CategoryId", "La catégorie sélectionnée n'existe pas.");
                CategoryOptions = await _context.CategoryStocks.OrderBy(c => c.Nom).ToListAsync();
                return Page();
            }

            _context.Stocks.Add(NewStock);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}