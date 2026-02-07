using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Cabinet.Pages.Consultations
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        private const int DefaultPageSize = 25;

        public IList<Consultation> Consultations { get; set; } = new List<Consultation>();
        public SelectList ServiceList { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ServiceFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowAll { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var services = await _context.Service
                .AsNoTracking()
                .OrderBy(s => s.NomService)
                .Select(s => new { s.IdService, s.NomService })
                .ToListAsync();
            ServiceList = new SelectList(services, "IdService", "NomService", ServiceFilter);

            var query = _context.Consultation
                .AsNoTracking()
                .Include(c => c.Patient)
                .Include(c => c.ServiceEntity)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                query = query.Where(c =>
                    (c.Patient != null && c.Patient.Nom != null && c.Patient.Nom.Contains(SearchString)) ||
                    (c.Patient != null && c.Patient.Prenom != null && c.Patient.Prenom.Contains(SearchString)) ||
                    (c.Etat != null && c.Etat.Contains(SearchString))
                );
            }

            if (DateFilter.HasValue)
            {
                query = query.Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value.Date == DateFilter.Value.Date);
            }
            else if (!ShowAll && string.IsNullOrWhiteSpace(SearchString) && !ServiceFilter.HasValue)
            {
                query = query.Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value.Date == DateTime.Today);
            }

            if (ServiceFilter.HasValue)
            {
                query = query.Where(c => c.ServiceId == ServiceFilter.Value);
            }

            query = query.OrderByDescending(c => c.DateConsultation);

            TotalCount = await query.CountAsync();
            var pageSize = ShowAll ? 200 : DefaultPageSize;
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)pageSize));
            PageNumber = Math.Min(Math.Max(1, PageNumber), TotalPages);

            Consultations = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var consultation = await _context.Consultation.FindAsync(id);
            if (consultation != null)
            {
                if (consultation.PatientId.HasValue && consultation.DateConsultation.HasValue)
                {
                    var hasPrescription = await _context.Ordonnance.AnyAsync(o =>
                        o.PatientID == consultation.PatientId.Value &&
                        o.DatePrescription.Date == consultation.DateConsultation.Value.Date);

                    if (hasPrescription)
                    {
                        TempData["ErrorMessage"] = "Suppression impossible: cette consultation possède une ordonnance associée.";
                        return RedirectToPage("./Index");
                    }
                }

                _context.Consultation.Remove(consultation);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}
