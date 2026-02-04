using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Rendezvous
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Cabinet.Models.Rendezvous Rendezvous { get; set; } = default!;

        public List<SelectListItem> ServiceOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadData();

            // Initialize with default values - round to nearest minute
            var now = DateTime.Now;
            Rendezvous = new Cabinet.Models.Rendezvous
            {
                DateHeure = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
                Nom = string.Empty,
                Prenom = string.Empty,
                Service = string.Empty
            };

            return Page();
        }

        private async Task LoadData()
        {
            ServiceOptions = await _context.Service
                .OrderBy(s => s.NomService)
                .Select(s => new SelectListItem
                {
                    Value = s.NomService,
                    Text = $"{s.NomService} ({s.Prix} DH)"
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove validation for properties we want to keep nullable
            ModelState.Remove("Rendezvous.Sexe");
            ModelState.Remove("Rendezvous.Phone");

            // Round DateHeure to nearest minute (remove seconds and milliseconds)
            if (Rendezvous != null)
            {
                Rendezvous.DateHeure = new DateTime(
                    Rendezvous.DateHeure.Year,
                    Rendezvous.DateHeure.Month,
                    Rendezvous.DateHeure.Day,
                    Rendezvous.DateHeure.Hour,
                    Rendezvous.DateHeure.Minute,
                    0
                );
            }

            _logger.LogInformation("POST received - Nom: {Nom}, Service: {Service}, DateHeure: {DateHeure}",
                Rendezvous?.Nom, Rendezvous?.Service, Rendezvous?.DateHeure);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        _logger.LogWarning("Key: {Key}, Error: {Error}",
                            modelState.Key, error.ErrorMessage);
                    }
                }

                await LoadData();
                return Page();
            }

            // Verify the service exists before saving
            var serviceExists = await _context.Service
                .AnyAsync(s => s.NomService == Rendezvous.Service);

            if (!serviceExists)
            {
                _logger.LogWarning("Service not found: {Service}", Rendezvous.Service);
                ModelState.AddModelError("Rendezvous.Service",
                    "Le service sélectionné n'existe pas dans la base de données.");
                await LoadData();
                return Page();
            }

            try
            {
                _context.Rendezvous.Add(Rendezvous);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rendezvous created successfully - ID: {Id}", Rendezvous.IdRdv);

                TempData["SuccessMessage"] = "Rendez-vous créé avec succès!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating rendezvous. Service: {Service}", Rendezvous.Service);

                // More specific error message
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty,
                    $"Erreur lors de la sauvegarde: {innerMessage}");
                await LoadData();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating rendezvous");
                ModelState.AddModelError(string.Empty,
                    "Une erreur inattendue s'est produite.");
                await LoadData();
                return Page();
            }
        }
    }
}