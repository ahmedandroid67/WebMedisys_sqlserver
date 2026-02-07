using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        public List<Consultation> PatientHistory { get; set; } = new();
        public string ServicePricesJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Consultation = await _context.Consultation
                .Include(c => c.Patient)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdConsultation == id.Value);

            if (Consultation == null) return NotFound();

            if (!Consultation.ServiceId.HasValue && !string.IsNullOrWhiteSpace(Consultation.Service))
            {
                Consultation.ServiceId = await _context.Service
                    .AsNoTracking()
                    .Where(s => s.NomService == Consultation.Service)
                    .Select(s => (int?)s.IdService)
                    .FirstOrDefaultAsync();
            }

            await LoadPageDependenciesAsync(Consultation.PatientId, Consultation.DateConsultation, Consultation.ServiceId);
            return Page();
        }

        public async Task<JsonResult> OnGetHistoryDetailsAsync(int id)
        {
            var history = await _context.Consultation
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdConsultation == id);

            if (history == null)
            {
                return new JsonResult(new { history = (object?)null, meds = new List<object>() });
            }

            var ord = await _context.Ordonnance
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.PatientID == history.PatientId
                                     && o.DatePrescription.Date == history.DateConsultation!.Value.Date);

            var meds = ord != null
                ? await (from om in _context.OrdonnanceMedicament.AsNoTracking()
                         join m in _context.Medicament.AsNoTracking() on om.MedicamentID equals m.Code
                         where om.OrdonnanceID == ord.OrdonnanceID
                         select new { m.Nom, m.Code, om.Quantite })
                    .Cast<object>()
                    .ToListAsync()
                : new List<object>();

            return new JsonResult(new { history, meds });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadPageDependenciesAsync(Consultation.PatientId, Consultation.DateConsultation, Consultation.ServiceId);
                return Page();
            }

            var consultationToUpdate = await _context.Consultation
                .FirstOrDefaultAsync(c => c.IdConsultation == Consultation.IdConsultation);

            if (consultationToUpdate == null)
            {
                return NotFound();
            }

            if (!Consultation.ServiceId.HasValue)
            {
                ModelState.AddModelError("Consultation.ServiceId", "Le service est obligatoire.");
                await LoadPageDependenciesAsync(Consultation.PatientId, Consultation.DateConsultation, Consultation.ServiceId);
                return Page();
            }

            var selectedService = await _context.Service
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdService == Consultation.ServiceId.Value);

            if (selectedService == null)
            {
                ModelState.AddModelError("Consultation.ServiceId", "Le service sélectionné est introuvable.");
                await LoadPageDependenciesAsync(Consultation.PatientId, Consultation.DateConsultation, Consultation.ServiceId);
                return Page();
            }

            consultationToUpdate.Etat = Consultation.Etat;
            consultationToUpdate.DateConsultation = Consultation.DateConsultation;
            consultationToUpdate.PatientId = Consultation.PatientId;
            consultationToUpdate.ServiceId = selectedService.IdService;
            consultationToUpdate.Service = selectedService.NomService;
            consultationToUpdate.PrixConsul = selectedService.Prix ?? 0;
            consultationToUpdate.Remise = Consultation.Remise;
            consultationToUpdate.MutRemplie = Consultation.MutRemplie;
            consultationToUpdate.Signe = Consultation.Signe;
            consultationToUpdate.Diagnostique = Consultation.Diagnostique;
            consultationToUpdate.Conduite = Consultation.Conduite;
            consultationToUpdate.TGly = Consultation.TGly;
            consultationToUpdate.TTension = Consultation.TTension;
            consultationToUpdate.TPoid = Consultation.TPoid;
            consultationToUpdate.TTaille = Consultation.TTaille;
            consultationToUpdate.TSpo = Consultation.TSpo;
            consultationToUpdate.TImc = Consultation.TImc;
            consultationToUpdate.TTemp = Consultation.TTemp;
            consultationToUpdate.TFvc = Consultation.TFvc;
            consultationToUpdate.TFev = Consultation.TFev;
            consultationToUpdate.TLdl = Consultation.TLdl;

            var medIds = Request.Form["medIds[]"].ToList();
            var posos = Request.Form["posos[]"].ToList();

            Ordonnance? ord = null;
            if (consultationToUpdate.PatientId.HasValue && consultationToUpdate.DateConsultation.HasValue)
            {
                ord = await _context.Ordonnance.FirstOrDefaultAsync(o =>
                    o.PatientID == consultationToUpdate.PatientId.Value &&
                    o.DatePrescription.Date == consultationToUpdate.DateConsultation.Value.Date);
            }

            if (medIds.Any())
            {
                if (ord == null)
                {
                    ord = new Ordonnance
                    {
                        PatientID = consultationToUpdate.PatientId ?? 0,
                        DatePrescription = consultationToUpdate.DateConsultation ?? DateTime.Now
                    };
                    _context.Ordonnance.Add(ord);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var oldMeds = _context.OrdonnanceMedicament.Where(x => x.OrdonnanceID == ord.OrdonnanceID);
                    _context.OrdonnanceMedicament.RemoveRange(oldMeds);
                    await _context.SaveChangesAsync();
                }

                foreach (var (medId, poso) in medIds.Zip(posos, (medId, poso) => (medId, poso)))
                {
                    _context.OrdonnanceMedicament.Add(new OrdonnanceMedicament
                    {
                        OrdonnanceID = ord.OrdonnanceID,
                        MedicamentID = medId,
                        Quantite = poso
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private async Task LoadPageDependenciesAsync(int? patientId, DateTime? consultationDate, int? selectedServiceId)
        {
            if (patientId.HasValue)
            {
                PatientHistory = await _context.Consultation
                    .AsNoTracking()
                    .Include(c => c.ServiceEntity)
                    .Where(c => c.PatientId == patientId.Value)
                    .OrderByDescending(c => c.DateConsultation)
                    .ToListAsync();
            }

            var meds = await _context.Medicament
                .AsNoTracking()
                .OrderBy(m => m.Nom)
                .Select(m => new { m.Code, Display = $"{m.Nom} {m.Dosage1}{m.UniteDosage1}" })
                .ToListAsync();
            MedicamentList = new SelectList(meds, "Code", "Display");

            var services = await _context.Service
                .AsNoTracking()
                .OrderBy(s => s.NomService)
                .Select(s => new { s.IdService, s.NomService, s.Prix })
                .ToListAsync();
            ServiceList = new SelectList(services, "IdService", "NomService", selectedServiceId);
            ServicePricesJson = JsonSerializer.Serialize(
                services.ToDictionary(
                    s => s.IdService.ToString(),
                    s => s.Prix ?? 0
                ));

            await LoadPrescriptions(patientId, consultationDate);
        }

        private async Task LoadPrescriptions(int? patientId, DateTime? date)
        {
            if (!date.HasValue || !patientId.HasValue)
            {
                return;
            }

            var ord = await _context.Ordonnance
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.PatientID == patientId.Value && o.DatePrescription.Date == date.Value.Date);

            if (ord != null)
            {
                ExistingPrescriptions = await (from om in _context.OrdonnanceMedicament.AsNoTracking()
                                               join m in _context.Medicament.AsNoTracking() on om.MedicamentID equals m.Code
                                               where om.OrdonnanceID == ord.OrdonnanceID
                                               select new { m.Code, m.Nom, m.Dosage1, m.UniteDosage1, om.Quantite })
                    .ToListAsync<dynamic>();
            }
        }
    }
}
