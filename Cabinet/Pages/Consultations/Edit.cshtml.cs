using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Consultations
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Consultation Consultation { get; set; } = default!;
        public SelectList MedicamentList { get; set; } = default!;
        public SelectList ServiceList { get; set; } = default!;
        public List<dynamic> ExistingPrescriptions { get; set; } = new();

        // NEW: Property to hold patient history
        public List<Consultation> PatientHistory { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Consultation = await _context.Consultation
                .Include(c => c.Patient)
                .FirstOrDefaultAsync(m => m.IdConsultation == id);

            if (Consultation == null) return NotFound();

            // Load History for the patient
            PatientHistory = await _context.Consultation
                .Where(c => c.PatientId == Consultation.PatientId )
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            var meds = await _context.Medicament
                .OrderBy(m => m.Nom)
                .Select(m => new { m.Code, Display = $"{m.Nom} {m.Dosage1}{m.UniteDosage1}" })
                .ToListAsync();
            MedicamentList = new SelectList(meds, "Code", "Display");

            var services = await _context.Service.OrderBy(s => s.NomService).ToListAsync();
            ServiceList = new SelectList(services, "Prix", "NomService");

            await LoadPrescriptions(Consultation.PatientId, Consultation.DateConsultation);

            return Page();
        }

        // NEW: AJAX Handler to fetch specific history details
        public async Task<JsonResult> OnGetHistoryDetailsAsync(int id)
        {
            var history = await _context.Consultation.FirstOrDefaultAsync(c => c.IdConsultation == id);

            var ord = await _context.Ordonnance
                .FirstOrDefaultAsync(o => o.PatientID == history.PatientId
                                     && o.DatePrescription.Date == history.DateConsultation.Value.Date);

            var meds = ord != null
    ? await (from om in _context.OrdonnanceMedicament
             join m in _context.Medicament on om.MedicamentID equals m.Code
             where om.OrdonnanceID == ord.OrdonnanceID
             select new { m.Nom, m.Code, om.Quantite })
              .Cast<object>() // This forces the anonymous type into an object list
              .ToListAsync()
    : new List<object>();

            return new JsonResult(new { history, meds });
        }

        private async Task LoadPrescriptions(int? patientId, DateTime? date)
        {
            if (!date.HasValue) return;
            var ord = await _context.Ordonnance.FirstOrDefaultAsync(o => o.PatientID == patientId && o.DatePrescription.Date == date.Value.Date);
            if (ord != null)
            {
                ExistingPrescriptions = await (from om in _context.OrdonnanceMedicament
                                               join m in _context.Medicament on om.MedicamentID equals m.Code
                                               where om.OrdonnanceID == ord.OrdonnanceID
                                               select new { m.Code, m.Nom, m.Dosage1, m.UniteDosage1, om.Quantite })
                                               .ToListAsync<dynamic>();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            _context.Attach(Consultation).State = EntityState.Modified;

            var medIds = Request.Form["medIds[]"].ToList();
            var posos = Request.Form["posos[]"].ToList();

            var ord = await _context.Ordonnance.FirstOrDefaultAsync(o => o.PatientID == Consultation.PatientId && o.DatePrescription.Date == Consultation.DateConsultation.Value.Date);

            if (medIds.Any())
            {
                if (ord == null)
                {
                    ord = new Ordonnance { PatientID = Consultation.PatientId ?? 0, DatePrescription = Consultation.DateConsultation.Value };
                    _context.Ordonnance.Add(ord);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var oldMeds = _context.OrdonnanceMedicament.Where(x => x.OrdonnanceID == ord.OrdonnanceID);
                    _context.OrdonnanceMedicament.RemoveRange(oldMeds);
                    await _context.SaveChangesAsync();
                }

                foreach (var (medId, poso) in medIds.Zip(posos))
                {
                    _context.OrdonnanceMedicament.Add(new OrdonnanceMedicament { OrdonnanceID = ord.OrdonnanceID, MedicamentID = medId, Quantite = poso });
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}