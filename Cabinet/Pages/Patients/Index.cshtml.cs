using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Patients
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Patient> Patients { get; set; } = default!;

        // Search String
        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public async Task OnGetAsync()
        {
            var patientsQuery = _context.Patient.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                // Search by Name, First Name, or CIN
                patientsQuery = patientsQuery.Where(s =>
                    s.Nom.Contains(SearchString) ||
                    s.Prenom.Contains(SearchString) ||
                    s.Cin.Contains(SearchString));
            }

            // Order by most recently added or by name
            Patients = await patientsQuery.OrderByDescending(p => p.IdPatient).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var patient = await _context.Patient.FindAsync(id);

            if (patient != null)
            {
                var hasConsultations = await _context.Consultation.AnyAsync(c => c.PatientId == id);
                var hasOrdonnances = await _context.Ordonnance.AnyAsync(o => o.PatientID == id);

                if (hasConsultations || hasOrdonnances)
                {
                    TempData["ErrorMessage"] = "Suppression impossible: ce patient possède des consultations ou des ordonnances.";
                    return RedirectToPage("./Index");
                }

                _context.Patient.Remove(patient);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
