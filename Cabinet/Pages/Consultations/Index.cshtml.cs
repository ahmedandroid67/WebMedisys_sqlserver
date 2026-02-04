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

        public IList<Cabinet.Models.Consultation> Consultations { get; set; } = new List<Cabinet.Models.Consultation>();
        public SelectList ServiceList { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ServiceFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowAll { get; set; }

        public async Task OnGetAsync()
        {
            // Load Services for the dropdown
            var services = await _context.Service.OrderBy(s => s.NomService).ToListAsync();
            ServiceList = new SelectList(services, "NomService", "NomService");

            var query = _context.Consultation
                .Include(c => c.Patient)
                .AsQueryable();

            // 1. Filter by Search String (Name/Etat)
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(c =>
                    (c.Patient != null && c.Patient.Nom.Contains(SearchString)) ||
                    (c.Patient != null && c.Patient.Prenom.Contains(SearchString)) ||
                    (c.Etat != null && c.Etat.Contains(SearchString))
                );
            }

            // 2. Filter by Date
            if (DateFilter.HasValue)
            {
                query = query.Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value.Date == DateFilter.Value.Date);
            }
            else if (!ShowAll && string.IsNullOrEmpty(SearchString) && string.IsNullOrEmpty(ServiceFilter))
            {
                // Default to Today if ShowAll is not checked and no specific search is performed
                query = query.Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value.Date == DateTime.Today);
            }

            // 3. Filter by Service
            if (!string.IsNullOrEmpty(ServiceFilter))
            {
                query = query.Where(c => c.Service == ServiceFilter);
            }

            // 4. Order and limit
            query = query.OrderByDescending(c => c.DateConsultation);
            
            // If ShowAll is checked, take more records (e.g., 200), otherwise limit to 50
            Consultations = await query
                .Take(ShowAll ? 200 : 50)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var consultation = await _context.Consultation.FindAsync(id);
            if (consultation != null)
            {
                _context.Consultation.Remove(consultation);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}