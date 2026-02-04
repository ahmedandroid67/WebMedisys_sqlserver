using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> OnGetAsync()
        {
            // Create 'Now' but set seconds and milliseconds to zero
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
                .OrderBy(p => p.Nom)
                .Select(p => new { p.IdPatient, Name = p.Nom + " " + p.Prenom })
                .ToListAsync();

            PatientList = new SelectList(patients, "IdPatient", "Name");

            var services = await _context.Service
                .OrderBy(s => s.NomService)
                .ToListAsync();

            ServiceList = new SelectList(services, "Prix", "NomService");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadData();
                return Page();
            }

            // 1. Identify service name
            var selectedPrice = Consultation.PrixConsul;
            var serviceEntry = await _context.Service.FirstOrDefaultAsync(s => s.Prix == selectedPrice);
            string serviceName = serviceEntry?.NomService ?? "Inconnu";

            // 2. Use user-selected date or fallback to Now
            if (!Consultation.DateConsultation.HasValue)
            {
                Consultation.DateConsultation = DateTime.Now;
            }

            var targetedDate = Consultation.DateConsultation.Value.Date;

            // 3. DUPLICATE CHECK: Using the selected date instead of strictly 'Today'
            var exists = await _context.Consultation.AnyAsync(c =>
                c.PatientId == Consultation.PatientId &&
                c.DateConsultation.Value.Date == targetedDate &&
                c.PrixConsul == selectedPrice);

            if (exists)
            {
                ModelState.AddModelError(string.Empty, $"Erreur : Ce patient a déjà un dossier pour ce service le {targetedDate:dd/MM/yyyy}.");
                await LoadData();
                return Page();
            }

            // 4. Finalize and Save
            Consultation.Etat = "Reception";
            Consultation.Service = serviceName;

            _context.Consultation.Add(Consultation);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}