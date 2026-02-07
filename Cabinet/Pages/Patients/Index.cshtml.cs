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
        private const int PageSize = 25;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Patient> Patients { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var patientsQuery = _context.Patient.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                patientsQuery = patientsQuery.Where(s =>
                    (s.Nom != null && s.Nom.Contains(SearchString)) ||
                    (s.Prenom != null && s.Prenom.Contains(SearchString)) ||
                    (s.Cin != null && s.Cin.Contains(SearchString)));
            }

            patientsQuery = patientsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.IdPatient);

            TotalCount = await patientsQuery.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            PageNumber = Math.Min(Math.Max(1, PageNumber), TotalPages);

            Patients = await patientsQuery
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
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
