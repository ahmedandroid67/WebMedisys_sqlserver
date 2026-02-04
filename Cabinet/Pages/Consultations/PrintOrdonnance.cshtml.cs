using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Consultations
{
    public class PrintOrdonnanceModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintOrdonnanceModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Consultation Consultation { get; set; } = default!;
        public List<dynamic> Prescriptions { get; set; } = new();

        // New property to hold dynamic cabinet info
        public CabinetInfo Cabinet { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Fetch Dynamic Cabinet Information
            Cabinet = await _context.CabinetInfo.FirstOrDefaultAsync() ?? new CabinetInfo
            {
                DrName = "Nom du Docteur",
                Speciality = "Spécialité",
                Address = "Adresse"
            };

            // 2. Fetch Consultation and Patient Info
            Consultation = await _context.Consultation
                .Include(c => c.Patient)
                .FirstOrDefaultAsync(m => m.IdConsultation == id);

            if (Consultation == null) return NotFound();

            // 3. Fetch the Ordonnance linked to this consultation date and patient
            var ord = await _context.Ordonnance
                .FirstOrDefaultAsync(o => o.PatientID == Consultation.PatientId
                                     && o.DatePrescription.Date == Consultation.DateConsultation.Value.Date);

            if (ord != null)
            {
                Prescriptions = await (from om in _context.OrdonnanceMedicament
                                       join m in _context.Medicament on om.MedicamentID equals m.Code
                                       where om.OrdonnanceID == ord.OrdonnanceID
                                       select new
                                       {
                                           m.Nom,
                                           m.Dosage1,
                                           m.UniteDosage1,
                                           om.Quantite
                                       }).ToListAsync<dynamic>();
            }

            return Page();
        }
    }
}