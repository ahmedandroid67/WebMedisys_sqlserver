using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Properties for Statistics ---
        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public decimal TodayRevenue { get; set; }

        // --- Properties for Lists ---
        // Using fully qualified names to avoid "namespace vs type" conflicts
        public List<Cabinet.Models.Consultation> WaitingList { get; set; } = new();
        public List<Cabinet.Models.Rendezvous> UpcomingRdv { get; set; } = new();
        public List<Cabinet.Models.Stock> LowStockItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;

            // 1. Fetch Top-Level Statistics using pluralized DbSet names
            TotalPatients = await _context.Patient.CountAsync();

            // FIX: Filter TodayAppointments to only count today's date
            TodayAppointments = await _context.Rendezvous
                .CountAsync(r => r.DateHeure.Date == today);

            // Calculate Today's Revenue (Consultation Price - Discount)
            TodayRevenue = await _context.Consultation
                .Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value.Date == today)
                .SumAsync(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0));

            // 2. Fetch the Waiting List (today only — Bug 4 fix)
            WaitingList = await _context.Consultation
                .Include(c => c.Patient)
                .Where(c => c.DateConsultation.HasValue
                    && c.DateConsultation.Value.Date == today
                    && (c.Etat == "Reception" || c.Etat == "Visite"))
                .OrderBy(c => c.DateConsultation)
                .ToListAsync();

            // 3. FIX: Fetch ONLY today's Agenda
            UpcomingRdv = await _context.Rendezvous
                .Where(r => r.DateHeure.Date == today)
                .OrderBy(r => r.DateHeure)
                .ToListAsync();

            // 4. Fetch Low Stock Items (Threshold check)
            LowStockItems = await _context.Stocks
                .Include(s => s.Category)
                .Where(s => s.Quantite <= s.Alarme)
                .OrderBy(s => s.Quantite)
                .Take(5)
                .ToListAsync();
        }

        // Improvement 4: advance Reception → Visite with one click
        public async Task<IActionResult> OnPostAdvanceStatusAsync(int id)
        {
            var consultation = await _context.Consultation.FindAsync(id);
            if (consultation != null && consultation.Etat == "Reception")
            {
                consultation.Etat = "Visite";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Patient passé en visite.";
            }
            return RedirectToPage();
        }

        // Improvement 4: mark consultation as Terminer with one click
        public async Task<IActionResult> OnPostTerminerAsync(int id)
        {
            var consultation = await _context.Consultation.FindAsync(id);
            if (consultation != null && (consultation.Etat == "Reception" || consultation.Etat == "Visite"))
            {
                consultation.Etat = "Terminer";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Consultation terminée avec succès.";
            }
            return RedirectToPage();
        }
    }
}
