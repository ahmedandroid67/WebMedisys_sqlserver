using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Administration
{
    public class CabinetSettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CabinetSettingsModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public CabinetInfo Info { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Load the existing info or create a new empty object if none exists
            Info = await _context.CabinetInfo.FirstOrDefaultAsync() ?? new CabinetInfo();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var existing = await _context.CabinetInfo.FirstOrDefaultAsync();

            if (existing == null)
            {
                _context.CabinetInfo.Add(Info);
            }
            else
            {
                // Map the new values to the existing tracked entity
                _context.Entry(existing).CurrentValues.SetValues(Info);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Informations mises à jour avec succès !";
            return RedirectToPage();
        }
    }
}