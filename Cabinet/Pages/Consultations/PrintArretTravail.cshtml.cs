using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Consultations
{
    public class PrintArretTravailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintArretTravailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Consultation Consultation { get; set; } = default!;
        public CabinetInfo Cabinet { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Load consultation with patient data
            var consultation = await _context.Consultation
                .Include(c => c.Patient)
                .FirstOrDefaultAsync(c => c.IdConsultation == id);

            if (consultation == null)
            {
                return NotFound();
            }

            Consultation = consultation;

            // Load cabinet information
            Cabinet = await _context.CabinetInfo.FirstOrDefaultAsync() 
                ?? new CabinetInfo 
                { 
                    DrName = "Dr. [Nom]",
                    Speciality = "[Spécialité]",
                    Address = "[Adresse]",
                    Phone = "[Téléphone]"
                };

            return Page();
        }
    }
}
