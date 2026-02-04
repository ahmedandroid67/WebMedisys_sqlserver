using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Medicaments
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Medicament Medicament { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(double? id)
        {
            if (id == null) return NotFound();

            var medicament = await _context.Medicament.FirstOrDefaultAsync(m => m.Id == id);

            if (medicament == null) return NotFound();

            Medicament = medicament;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            _context.Attach(Medicament).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!MedicamentExists(Medicament.Id ?? 0))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool MedicamentExists(double id)
        {
            return _context.Medicament.Any(e => e.Id == id);
        }
    }
}