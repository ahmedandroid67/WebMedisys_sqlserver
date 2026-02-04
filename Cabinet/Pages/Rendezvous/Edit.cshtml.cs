using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Rendezvous
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Cabinet.Models.Rendezvous Rendezvous { get; set; } = default!;

        public List<SelectListItem> ServiceOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rendezvous = await _context.Rendezvous.FirstOrDefaultAsync(m => m.IdRdv == id);
            if (rendezvous == null)
            {
                return NotFound();
            }

            Rendezvous = rendezvous;

            // Round to nearest minute for display
            Rendezvous.DateHeure = new DateTime(
                Rendezvous.DateHeure.Year,
                Rendezvous.DateHeure.Month,
                Rendezvous.DateHeure.Day,
                Rendezvous.DateHeure.Hour,
                Rendezvous.DateHeure.Minute,
                0
            );

            await LoadData();
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
            // Remove validation for nullable properties
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

            _logger.LogInformation("POST received for edit - ID: {Id}, Nom: {Nom}, Service: {Service}",
                Rendezvous?.IdRdv, Rendezvous?.Nom, Rendezvous?.Service);

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

            // Verify the service exists
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

            _context.Attach(Rendezvous).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Rendezvous updated successfully - ID: {Id}", Rendezvous.IdRdv);

                TempData["SuccessMessage"] = "Rendez-vous modifié avec succès!";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RendezvousExists(Rendezvous.IdRdv))
                {
                    _logger.LogWarning("Rendezvous not found during update - ID: {Id}", Rendezvous.IdRdv);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating rendezvous");
                    ModelState.AddModelError(string.Empty,
                        "Une erreur de concurrence s'est produite. Veuillez réessayer.");
                    await LoadData();
                    return Page();
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating rendezvous");
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty,
                    $"Erreur lors de la sauvegarde: {innerMessage}");
                await LoadData();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating rendezvous");
                ModelState.AddModelError(string.Empty,
                    "Une erreur inattendue s'est produite.");
                await LoadData();
                return Page();
            }
        }

        private bool RendezvousExists(int id)
        {
            return _context.Rendezvous.Any(e => e.IdRdv == id);
        }
    }
}