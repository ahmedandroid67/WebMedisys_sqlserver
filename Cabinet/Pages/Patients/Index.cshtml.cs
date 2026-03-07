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
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _environment;
        private const int PageSize = 25;
        private const int MaxHistoryRows = 100;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
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

        public async Task<IActionResult> OnGetHistoryAsync(int id)
        {
            try
            {
                var patient = await _context.Patient
                    .AsNoTracking()
                    .Where(p => p.IdPatient == id)
                    .Select(p => new
                    {
                        p.IdPatient,
                        p.Nom,
                        p.Prenom,
                        p.Cin,
                        p.Phone,
                        p.Email,
                        p.Sexe,
                        p.DateNaiss,
                        p.Adresse
                    })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    return NotFound();
                }

                var consultations = await _context.Consultation
                    .AsNoTracking()
                    .Where(c => c.PatientId == id)
                    .OrderByDescending(c => c.DateConsultation ?? c.CreatedAt)
                    .Take(MaxHistoryRows)
                    .Select(c => new
                    {
                        c.IdConsultation,
                        c.DateConsultation,
                        c.Etat,
                        c.Service,
                        c.PrixConsul,
                        c.Remise,
                        c.PaymentMethod,
                        c.PaymentDate,
                        c.ReceiptNumber,
                        c.Signe,
                        c.Diagnostique,
                        c.Conduite,
                        c.TGly,
                        c.TTension,
                        c.TPoid,
                        c.TTaille,
                        c.TSpo,
                        c.TImc,
                        c.TTemp,
                        c.TFvc,
                        c.TFev,
                        c.TLdl
                    })
                    .ToListAsync();

                var ordonnances = await _context.Ordonnance
                    .AsNoTracking()
                    .Where(o => o.PatientID == id)
                    .OrderByDescending(o => o.DatePrescription)
                    .Take(MaxHistoryRows)
                    .Select(o => new
                    {
                        o.OrdonnanceID,
                        o.DatePrescription
                    })
                    .ToListAsync();

                var ordonnanceIds = ordonnances.Select(o => o.OrdonnanceID).ToList();
                var ordonnanceMedicaments = new Dictionary<int, List<object>>();

                if (ordonnanceIds.Count > 0)
                {
                    // Avoid list-Contains SQL translation for older SQL Server compatibility levels.
                    var medicationRows = await (from o in _context.Ordonnance.AsNoTracking()
                                                join om in _context.OrdonnanceMedicament.AsNoTracking()
                                                    on o.OrdonnanceID equals om.OrdonnanceID
                                                join m in _context.Medicament.AsNoTracking()
                                                    on om.MedicamentID equals m.Code into meds
                                                from med in meds.DefaultIfEmpty()
                                                where o.PatientID == id
                                                select new
                                                {
                                                    om.OrdonnanceID,
                                                    om.MedicamentID,
                                                    om.Quantite,
                                                    MedicationName = med != null ? med.Nom : null,
                                                    Dosage1 = med != null ? med.Dosage1 : null,
                                                    UniteDosage1 = med != null ? med.UniteDosage1 : null
                                                })
                        .ToListAsync();

                    var selectedOrdonnanceIds = ordonnanceIds.ToHashSet();
                    medicationRows = medicationRows
                        .Where(m => selectedOrdonnanceIds.Contains(m.OrdonnanceID))
                        .Select(om => new
                        {
                            om.OrdonnanceID,
                            om.MedicamentID,
                            om.Quantite,
                            om.MedicationName,
                            om.Dosage1,
                            om.UniteDosage1
                        })
                        .ToList();

                    // Handle duplicate medicament code rows by keeping a single best row.
                    medicationRows = medicationRows
                        .GroupBy(x => new { x.OrdonnanceID, x.MedicamentID, x.Quantite })
                        .Select(g => g
                            .OrderByDescending(x => !string.IsNullOrWhiteSpace(x.MedicationName))
                            .First())
                        .ToList();

                    ordonnanceMedicaments = medicationRows
                        .GroupBy(m => m.OrdonnanceID)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x =>
                            {
                                return (object)new
                                {
                                    MedicationName = string.IsNullOrWhiteSpace(x.MedicationName) ? x.MedicamentID : x.MedicationName,
                                    x.Dosage1,
                                    x.UniteDosage1,
                                    x.Quantite
                                };
                            }).ToList());
                }

                var ordonnancesPayload = ordonnances.Select(o => new
                {
                    o.OrdonnanceID,
                    o.DatePrescription,
                    Medicaments = ordonnanceMedicaments.TryGetValue(o.OrdonnanceID, out var meds)
                        ? meds
                        : new List<object>()
                });

                var lastDiagnosis = consultations
                    .Select(c => c.Diagnostique)
                    .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d));

                return new JsonResult(new
                {
                    Patient = patient,
                    Summary = new
                    {
                        TotalConsultations = consultations.Count,
                        TotalOrdonnances = ordonnances.Count,
                        LastConsultationDate = consultations.FirstOrDefault()?.DateConsultation,
                        LastDiagnosis = lastDiagnosis
                    },
                    Consultations = consultations,
                    Ordonnances = ordonnancesPayload
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed loading patient history for patientId={PatientId}", id);
                if (_environment.IsDevelopment())
                {
                    return StatusCode(500, new
                    {
                        message = "Erreur interne lors du chargement de l'historique du patient.",
                        debug = ex.Message
                    });
                }

                return StatusCode(500, new { message = "Erreur interne lors du chargement de l'historique du patient." });
            }
        }
    }
}
