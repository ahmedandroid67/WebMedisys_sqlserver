using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace Cabinet.Pages.Reports
{
    [Authorize]
    public class StatisticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StatisticsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Patient Statistics
        public int TotalPatients { get; set; }
        public int NewPatientsThisMonth { get; set; }
        public int NewPatientsThisYear { get; set; }
        public List<PatientAgeGroupDto> PatientsByAgeGroup { get; set; } = new();
        public Dictionary<string, int> PatientsByGender { get; set; } = new();

        // Consultation Statistics
        public int TotalConsultationsToday { get; set; }
        public int TotalConsultationsThisMonth { get; set; }
        public int TotalConsultationsThisYear { get; set; }
        public List<ConsultationByMonthDto> ConsultationsByMonth { get; set; } = new();
        public List<ServiceStatDto> TopServices { get; set; } = new();

        // Stock Statistics
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<LowStockItemDto> LowStockItems { get; set; } = new();

        // Financial Statistics
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }
        public decimal AverageConsultationPrice { get; set; }
        
        public CabinetInfo? CabinetInfo { get; set; }

        public async Task OnGetAsync()
        {
            await LoadStatisticsData();
        }

        public async Task<IActionResult> OnGetExportExcelAsync()
        {
            await LoadStatisticsData();

            using var workbook = new XLWorkbook();
            
            // Sheet 1: Patients
            var wsPatients = workbook.Worksheets.Add("Patients");
            wsPatients.Cell("A1").Value = "RÉPARTITION PAR TRANCHE D'ÂGE";
            wsPatients.Cell("A1").Style.Font.Bold = true;
            wsPatients.Range("A1:B1").Merge();
            
            wsPatients.Cell("A2").Value = "Tranche d'âge";
            wsPatients.Cell("B2").Value = "Nombre";
            wsPatients.Range("A2:B2").Style.Font.Bold = true;
            wsPatients.Range("A2:B2").Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 3;
            foreach (var item in PatientsByAgeGroup)
            {
                wsPatients.Cell($"A{row}").Value = item.AgeGroup;
                wsPatients.Cell($"B{row}").Value = item.Count;
                row++;
            }

            row += 2;
            wsPatients.Cell($"A{row}").Value = "RÉPARTITION PAR SEXE";
            wsPatients.Cell($"A{row}").Style.Font.Bold = true;
            wsPatients.Range($"A{row}:B{row}").Merge();
            row++;
            
            wsPatients.Cell($"A{row}").Value = "Sexe";
            wsPatients.Cell($"B{row}").Value = "Nombre";
            wsPatients.Range($"A{row}:B{row}").Style.Font.Bold = true;
            row++;

            foreach (var gender in PatientsByGender)
            {
                wsPatients.Cell($"A{row}").Value = gender.Key;
                wsPatients.Cell($"B{row}").Value = gender.Value;
                row++;
            }
            wsPatients.Columns().AdjustToContents();

            // Sheet 2: Consultations & Revenue
            var wsConsults = workbook.Worksheets.Add("Consultations & Revenus");
            wsConsults.Cell("A1").Value = "ÉVOLUTION MENSUELLE";
            wsConsults.Cell("A1").Style.Font.Bold = true;
            wsConsults.Cell("A2").Value = "Mois";
            wsConsults.Cell("B2").Value = "Consultations";
            wsConsults.Cell("C2").Value = "Revenu";
            wsConsults.Range("A2:C2").Style.Font.Bold = true;

            row = 3;
            foreach (var month in ConsultationsByMonth)
            {
                wsConsults.Cell($"A{row}").Value = month.MonthName;
                wsConsults.Cell($"B{row}").Value = month.Count;
                wsConsults.Cell($"C{row}").Value = month.Revenue;
                wsConsults.Cell($"C{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
                row++;
            }
            wsConsults.Columns().AdjustToContents();

            // Sheet 3: Services
            var wsServices = workbook.Worksheets.Add("Top Services");
            wsServices.Cell("A1").Value = "TOPS SERVICES";
            wsServices.Cell("A1").Style.Font.Bold = true;
            wsServices.Cell("A2").Value = "Service";
            wsServices.Cell("B2").Value = "Nombre";
            wsServices.Cell("C2").Value = "Revenu";
            wsServices.Range("A2:C2").Style.Font.Bold = true;

            row = 3;
            foreach (var service in TopServices)
            {
                wsServices.Cell($"A{row}").Value = service.ServiceName;
                wsServices.Cell($"B{row}").Value = service.Count;
                wsServices.Cell($"C{row}").Value = service.Revenue;
                wsServices.Cell($"C{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
                row++;
            }
            wsServices.Columns().AdjustToContents();

            // Sheet 4: Inventaire (Alerte Stock)
            var wsStock = workbook.Worksheets.Add("Alertes Stock");
            wsStock.Cell("A1").Value = "PRODUITS EN STOCK BAS OU RUPTURE";
            wsStock.Cell("A1").Style.Font.Bold = true;
            wsStock.Cell("A2").Value = "Produit";
            wsStock.Cell("B2").Value = "Quantité";
            wsStock.Cell("C2").Value = "Seuil Alerte";
            wsStock.Range("A2:C2").Style.Font.Bold = true;

            row = 3;
            foreach (var item in LowStockItems)
            {
                wsStock.Cell($"A{row}").Value = item.ProductName;
                wsStock.Cell($"B{row}").Value = item.Quantity;
                wsStock.Cell($"C{row}").Value = item.AlertLevel;
                if (item.Quantity == 0) wsStock.Cell($"A{row}").Style.Font.FontColor = XLColor.Red;
                row++;
            }
            wsStock.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Statistiques_Cabinet_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
        }

        private async Task LoadStatisticsData()
        {
            CabinetInfo = await _context.CabinetInfo.AsNoTracking().FirstOrDefaultAsync();

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfYear = new DateTime(today.Year, 1, 1);

            // Patient Statistics
            var allPatients = await _context.Patient.AsNoTracking().ToListAsync();
            TotalPatients = allPatients.Count;
            
            NewPatientsThisMonth = allPatients.Count(p =>
                p.CreatedAt.Year == today.Year && p.CreatedAt.Month == today.Month);
            
            NewPatientsThisYear = allPatients.Count(p => p.CreatedAt.Year == today.Year);

            // Gender distribution
            PatientsByGender = allPatients
                .Where(p => !string.IsNullOrEmpty(p.Sexe))
                .GroupBy(p => p.Sexe!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Age groups
            PatientsByAgeGroup = allPatients
                .Where(p => p.DateNaiss.HasValue)
                .Select(p => new
                {
                    Age = today.Year - p.DateNaiss!.Value.Year
                })
                .GroupBy(p => p.Age < 18 ? "0-17" : p.Age < 35 ? "18-34" : p.Age < 50 ? "35-49" : p.Age < 65 ? "50-64" : "65+")
                .Select(g => new PatientAgeGroupDto
                {
                    AgeGroup = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.AgeGroup)
                .ToList();

            // Consultation Statistics
            var consultations = await _context.Consultation
                .AsNoTracking()
                .Include(c => c.ServiceEntity)
                .ToListAsync();

            TotalConsultationsToday = consultations.Count(c => c.DateConsultation.HasValue && 
                c.DateConsultation.Value.Date == today);
            
            TotalConsultationsThisMonth = consultations.Count(c => c.DateConsultation.HasValue && 
                c.DateConsultation.Value >= firstDayOfMonth);
            
            TotalConsultationsThisYear = consultations.Count(c => c.DateConsultation.HasValue && 
                c.DateConsultation.Value >= firstDayOfYear);

            // Consultations by month (last 12 months)
            ConsultationsByMonth = consultations
                .Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value >= today.AddMonths(-11))
                .GroupBy(c => new { c.DateConsultation!.Value.Year, c.DateConsultation.Value.Month })
                .Select(g => new ConsultationByMonthDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0))
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();

            // Top services
            TopServices = consultations
                .Where(c => c.ServiceId.HasValue || !string.IsNullOrEmpty(c.Service))
                .GroupBy(c => new
                {
                    c.ServiceId,
                    Name = c.ServiceEntity != null ? c.ServiceEntity.NomService : c.Service
                })
                .Select(g => new ServiceStatDto
                {
                    ServiceName = g.Key.Name ?? "Inconnu",
                    Count = g.Count(),
                    Revenue = g.Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0))
                })
                .OrderByDescending(s => s.Count)
                .Take(10)
                .ToList();

            // Financial Statistics
            RevenueToday = consultations
                .Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value.Date == today)
                .Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0));

            RevenueThisMonth = consultations
                .Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value >= firstDayOfMonth)
                .Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0));

            RevenueThisYear = consultations
                .Where(c => c.DateConsultation.HasValue && c.DateConsultation.Value >= firstDayOfYear)
                .Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0));

            var consultationsWithPrice = consultations.Where(c => c.PrixConsul.HasValue).ToList();
            AverageConsultationPrice = consultationsWithPrice.Any() 
                ? consultationsWithPrice.Average(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0)) 
                : 0;

            // Stock Statistics
            var stockItems = await _context.Stocks.AsNoTracking().ToListAsync();
            TotalProducts = stockItems.Count;
            LowStockProducts = stockItems.Count(s => s.Quantite <= s.Alarme && s.Quantite > 0);
            OutOfStockProducts = stockItems.Count(s => s.Quantite == 0);

            LowStockItems = stockItems
                .Where(s => s.Quantite <= s.Alarme)
                .OrderBy(s => s.Quantite)
                .Take(10)
                .Select(s => new LowStockItemDto
                {
                    ProductName = s.Nom ?? "N/A",
                    Quantity = s.Quantite,
                    AlertLevel = s.Alarme
                })
                .ToList();
        }
    }

    public class PatientAgeGroupDto
    {
        public string AgeGroup { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ConsultationByMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }

    public class ServiceStatDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int AlertLevel { get; set; }
    }
}
