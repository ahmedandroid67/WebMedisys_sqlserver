using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Cabinet.Pages.Consultations
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Consultation Consultation { get; set; } = default!;

        public SelectList PatientList { get; set; } = default!;
        public SelectList ServiceList { get; set; } = default!;
        public string ServicePricesJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync()
        {
            var now = DateTime.Now;
            Consultation = new Consultation
            {
                DateConsultation = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)
            };

            await LoadData();
            return Page();
        }

        private async Task LoadData()
        {
            var patients = await _context.Patient
                .AsNoTracking()
                .OrderBy(p => p.Nom)
                .Select(p => new { p.IdPatient, Name = p.Nom + " " + p.Prenom })
                .ToListAsync();

            PatientList = new SelectList(patients, "IdPatient", "Name");

            var services = await _context.Service
                .AsNoTracking()
                .OrderBy(s => s.NomService)
                .Select(s => new { s.IdService, s.NomService, s.Prix })
                .ToListAsync();

            ServiceList = new SelectList(services, "IdService", "NomService");
            ServicePricesJson = JsonSerializer.Serialize(
                services.ToDictionary(
                    s => s.IdService.ToString(),
                    s => s.Prix ?? 0
                ));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadData();
                return Page();
            }

            if (!Consultation.ServiceId.HasValue)
            {
                ModelState.AddModelError("Consultation.ServiceId", "Le service est obligatoire.");
                await LoadData();
                return Page();
            }

            var serviceEntry = await _context.Service
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdService == Consultation.ServiceId.Value);

            if (serviceEntry == null)
            {
                ModelState.AddModelError("Consultation.ServiceId", "Le service sélectionné est introuvable.");
                await LoadData();
                return Page();
            }

            if (!Consultation.DateConsultation.HasValue)
            {
                Consultation.DateConsultation = DateTime.Now;
            }

            var targetedDate = Consultation.DateConsultation.Value.Date;

            var exists = await _context.Consultation.AnyAsync(c =>
                c.PatientId == Consultation.PatientId &&
                c.DateConsultation.HasValue &&
                c.DateConsultation.Value.Date == targetedDate &&
                c.ServiceId == Consultation.ServiceId);

            if (exists)
            {
                ModelState.AddModelError(string.Empty, $"Erreur : Ce patient a déjà un dossier pour ce service le {targetedDate:dd/MM/yyyy}.");
                await LoadData();
                return Page();
            }

            Consultation.Etat = "Reception";
            Consultation.Service = serviceEntry.NomService;
            Consultation.PrixConsul = serviceEntry.Prix ?? 0;

            _context.Consultation.Add(Consultation);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
