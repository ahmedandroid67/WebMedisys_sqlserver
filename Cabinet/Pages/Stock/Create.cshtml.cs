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
                // If there's an error, reload the categories and stay on the page
                CategoryOptions = await _context.CategoryStocks.OrderBy(c => c.Nom).ToListAsync();
                return Page();
            }

            // Add the new product to the database
            _context.Stocks.Add(NewStock);
            await _context.SaveChangesAsync();

            // Redirect back to the stock list after success
            return RedirectToPage("./Index");
        }
    }
}