using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Pages.Consultations
{
    public class PrintRecuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintRecuModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Consultation Consultation { get; set; } = default!;
        public CabinetInfo Cabinet { get; set; } = default!;
        public string ReceiptNumber { get; set; } = string.Empty;

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

            // Generate or retrieve receipt number
            if (string.IsNullOrEmpty(consultation.ReceiptNumber))
            {
                // Generate receipt number: YEAR-MONTH-CONSULTATIONID
                ReceiptNumber = $"{DateTime.Now:yyyyMM}-{consultation.IdConsultation:D6}";
                
                // Save receipt number to database
                consultation.ReceiptNumber = ReceiptNumber;
                if (consultation.PaymentDate == null)
                {
                    consultation.PaymentDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                ReceiptNumber = consultation.ReceiptNumber;
            }

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
