using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cabinet.Pages.Medicaments
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Medicament Medicament { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if ID already exists manually because it's not Auto-Increment
            if (_context.Medicament.Any(m => m.Id == Medicament.Id))
            {
                ModelState.AddModelError("Medicament.Id", "Cet ID existe déjà.");
                return Page();
            }

            _context.Medicament.Add(Medicament);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}