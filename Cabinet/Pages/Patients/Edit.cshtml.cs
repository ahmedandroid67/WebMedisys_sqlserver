using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Patients
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
        public Patient Patient { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patient.FirstOrDefaultAsync(m => m.IdPatient == id);

            if (patient == null) return NotFound();

            Patient = patient;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _context.Patient.FirstOrDefaultAsync(p => p.IdPatient == Patient.IdPatient);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Nom = Patient.Nom;
            existing.Prenom = Patient.Prenom;
            existing.Cin = Patient.Cin;
            existing.Email = Patient.Email;
            existing.Phone = Patient.Phone;
            existing.DateNaiss = Patient.DateNaiss;
            existing.Sexe = Patient.Sexe;
            existing.Adresse = Patient.Adresse;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(Patient.IdPatient))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool PatientExists(int id)
        {
            return _context.Patient.Any(e => e.IdPatient == id);
        }
    }
}