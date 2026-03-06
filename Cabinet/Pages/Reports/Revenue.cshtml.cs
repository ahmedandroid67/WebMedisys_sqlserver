using Cabinet.Data;
using Cabinet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Cabinet.Pages.Reports
{
    [Authorize]
    public class RevenueModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RevenueModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "today";

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // Statistics
        public decimal TotalRevenue { get; set; }
        public int TotalConsultations { get; set; }
        public decimal AverageConsultation { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal NetRevenue { get; set; }

        // Payment methods breakdown
        public Dictionary<string, decimal> PaymentMethodsBreakdown { get; set; } = new();

        // Services breakdown
        public List<ServiceRevenueDto> ServicesBreakdown { get; set; } = new();

        // Daily revenue for charts
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();

        // Detailed consultations list
        public List<Consultation> DetailedConsultations { get; set; } = new();

        public CabinetInfo? CabinetInfo { get; set; }

        public async Task OnGetAsync()
        {
            await LoadRevenueData();
        }

        public async Task<IActionResult> OnGetExportExcelAsync(string period = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            Period = period;
            StartDate = startDate;
            EndDate = endDate;
            
            await LoadRevenueData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rapport Revenus");

            // Title
            worksheet.Cell("A1").Value = "RAPPORT DE REVENUS - CABINET MÉDICAL";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 16;
            worksheet.Range("A1:F1").Merge();

            // Period
            worksheet.Cell("A2").Value = $"Période: Du {StartDate?.ToString("dd/MM/yyyy")} au {EndDate?.ToString("dd/MM/yyyy")}";
            worksheet.Range("A2:F2").Merge();

            // Statistics Section
            int row = 4;
            worksheet.Cell($"A{row}").Value = "STATISTIQUES GÉNÉRALES";
            worksheet.Cell($"A{row}").Style.Font.Bold = true;
            worksheet.Cell($"A{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range($"A{row}:B{row}").Merge();
            row++;

            worksheet.Cell($"A{row}").Value = "Revenu Total";
            worksheet.Cell($"B{row}").Value = TotalRevenue;
            worksheet.Cell($"B{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
            row++;

            worksheet.Cell($"A{row}").Value = "Remises";
            worksheet.Cell($"B{row}").Value = TotalDiscounts;
            worksheet.Cell($"B{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
            row++;

            worksheet.Cell($"A{row}").Value = "Revenu Net";
            worksheet.Cell($"B{row}").Value = NetRevenue;
            worksheet.Cell($"B{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
            worksheet.Cell($"B{row}").Style.Font.Bold = true;
            row++;

            worksheet.Cell($"A{row}").Value = "Nombre de Consultations";
            worksheet.Cell($"B{row}").Value = TotalConsultations;
            row++;

            worksheet.Cell($"A{row}").Value = "Moyenne par Consultation";
            worksheet.Cell($"B{row}").Value = AverageConsultation;
            worksheet.Cell($"B{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
            row += 2;

            // Payment Methods Section
            worksheet.Cell($"A{row}").Value = "MODES DE PAIEMENT";
            worksheet.Cell($"A{row}").Style.Font.Bold = true;
            worksheet.Cell($"A{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range($"A{row}:B{row}").Merge();
            row++;

            foreach (var payment in PaymentMethodsBreakdown.OrderByDescending(p => p.Value))
            {
                worksheet.Cell($"A{row}").Value = payment.Key;
                worksheet.Cell($"B{row}").Value = payment.Value;
                worksheet.Cell($"B{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
                row++;
            }
            row++;

            // Services Section
            worksheet.Cell($"A{row}").Value = "REVENUS PAR SERVICE";
            worksheet.Cell($"A{row}").Style.Font.Bold = true;
            worksheet.Cell($"A{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range($"A{row}:C{row}").Merge();
            row++;

            worksheet.Cell($"A{row}").Value = "Service";
            worksheet.Cell($"B{row}").Value = "Nombre";
            worksheet.Cell($"C{row}").Value = "Revenu";
            worksheet.Range($"A{row}:C{row}").Style.Font.Bold = true;
            row++;

            foreach (var service in ServicesBreakdown)
            {
                worksheet.Cell($"A{row}").Value = service.ServiceName;
                worksheet.Cell($"B{row}").Value = service.Count;
                worksheet.Cell($"C{row}").Value = service.TotalRevenue;
                worksheet.Cell($"C{row}").Style.NumberFormat.Format = "#,##0.00 \"DH\"";
                row++;
            }
            row += 2;

            // Detailed Consultations
            worksheet.Cell($"A{row}").Value = "DÉTAIL DES CONSULTATIONS";
            worksheet.Cell($"A{row}").Style.Font.Bold = true;
            worksheet.Cell($"A{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range($"A{row}:F{row}").Merge();
            row++;

            // Headers
            worksheet.Cell($"A{row}").Value = "Date";
            worksheet.Cell($"B{row}").Value = "Patient";
            worksheet.Cell($"C{row}").Value = "Service";
            worksheet.Cell($"D{row}").Value = "Prix";
            worksheet.Cell($"E{row}").Value = "Remise";
            worksheet.Cell($"F{row}").Value = "Net";
            worksheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
            worksheet.Range($"A{row}:F{row}").Style.Fill.BackgroundColor = XLColor.LightBlue;
            row++;

            foreach (var consultation in DetailedConsultations)
            {
                var net = (consultation.PrixConsul ?? 0) - (consultation.Remise ?? 0);
                worksheet.Cell($"A{row}").Value = consultation.DateConsultation?.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell($"B{row}").Value = $"{consultation.Patient?.Nom} {consultation.Patient?.Prenom}";
                worksheet.Cell($"C{row}").Value = consultation.ServiceEntity?.NomService ?? consultation.Service;
                worksheet.Cell($"D{row}").Value = consultation.PrixConsul ?? 0;
                worksheet.Cell($"D{row}").Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell($"E{row}").Value = consultation.Remise ?? 0;
                worksheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell($"F{row}").Value = net;
                worksheet.Cell($"F{row}").Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = $"Rapport_Revenus_{StartDate?.ToString("yyyyMMdd")}_{EndDate?.ToString("yyyyMMdd")}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Improvement 7: PDF Export using QuestPDF
        public async Task<IActionResult> OnGetExportPdfAsync(string period = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Period = period;
            StartDate = startDate;
            EndDate = endDate;
            await LoadRevenueData();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text(CabinetInfo?.DrName ?? "Cabinet Médical").Bold().FontSize(16).FontColor(Colors.Indigo.Darken2);
                                inner.Item().Text(CabinetInfo?.Speciality ?? "").FontSize(11).FontColor(Colors.Grey.Darken1);
                                inner.Item().Text(CabinetInfo?.Address ?? "").FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(130).Column(inner =>
                            {
                                inner.Item().AlignRight().Text("RAPPORT DE REVENUS").Bold().FontSize(12).FontColor(Colors.Indigo.Darken2);
                                inner.Item().AlignRight().Text($"Du {StartDate:dd/MM/yyyy} au {EndDate:dd/MM/yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                        col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Indigo.Lighten3);
                    });

                    page.Content().PaddingTop(16).Column(col =>
                    {
                        // KPI Summary
                        col.Item().Text("Résumé Financier").Bold().FontSize(12).FontColor(Colors.Indigo.Darken1);
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                            void Row(string label, string value, bool bold = false)
                            {
                                table.Cell().Padding(5).Text(label).FontColor(bold ? Colors.Black : Colors.Grey.Darken2);
                                table.Cell().Padding(5).AlignRight().Text(value).Bold();
                            }
                            Row("Revenu Total", $"{TotalRevenue:N2} DH");
                            Row("Total Remises", $"-{TotalDiscounts:N2} DH");
                            Row("Revenu Net", $"{NetRevenue:N2} DH", true);
                            Row("Consultations", TotalConsultations.ToString());
                            Row("Moyenne / Consultation", $"{AverageConsultation:N2} DH");
                        });

                        col.Item().PaddingTop(16).Text("Répartition par Service").Bold().FontSize(12).FontColor(Colors.Indigo.Darken1);
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(1); c.RelativeColumn(2); });
                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Indigo.Lighten4).Padding(5).Text("Service").Bold();
                                h.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignCenter().Text("Nb.").Bold();
                                h.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight().Text("Revenu").Bold();
                            });
                            foreach (var s in ServicesBreakdown)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(s.ServiceName);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(s.Count.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{s.TotalRevenue:N2} DH");
                            }
                        });

                        col.Item().PaddingTop(16).Text("Détail des Consultations").Bold().FontSize(12).FontColor(Colors.Indigo.Darken1);
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.ConstantColumn(60); c.RelativeColumn(3); c.RelativeColumn(2); c.ConstantColumn(55); c.ConstantColumn(50); c.ConstantColumn(60); });
                            table.Header(h =>
                            {
                                foreach (var label in new[] { "Date", "Patient", "Service", "Prix", "Remise", "Net" })
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(label).Bold().FontSize(9);
                            });
                            foreach (var c in DetailedConsultations)
                            {
                                var net = (c.PrixConsul ?? 0) - (c.Remise ?? 0);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.DateConsultation?.ToString("dd/MM/yy")).FontSize(9);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{c.Patient?.Nom} {c.Patient?.Prenom}").FontSize(9);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.ServiceEntity?.NomService ?? c.Service ?? "").FontSize(9);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{c.PrixConsul ?? 0:N0}").FontSize(9);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{c.Remise ?? 0:N0}").FontSize(9);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{net:N0}").FontSize(9).Bold();
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                        t.Span($"  —  Généré le {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Medium);
                    });
                });
            });

            var pdfBytes = pdf.GeneratePdf();
            var fileName = $"Revenue_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private async Task LoadRevenueData()
        {
            CabinetInfo = await _context.CabinetInfo.AsNoTracking().FirstOrDefaultAsync();

            // Calculate date range based on period
            var (start, end) = GetDateRange();
            StartDate = start;
            EndDate = end;

            // Get consultations in date range
            var consultations = await _context.Consultation
                .AsNoTracking()
                .Include(c => c.Patient)
                .Include(c => c.ServiceEntity)
                .Where(c => c.DateConsultation.HasValue 
                    && c.DateConsultation.Value.Date >= start.Date 
                    && c.DateConsultation.Value.Date <= end.Date)
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            DetailedConsultations = consultations;

            // Calculate statistics
            TotalConsultations = consultations.Count;
            TotalRevenue = consultations.Sum(c => c.PrixConsul ?? 0);
            TotalDiscounts = consultations.Sum(c => c.Remise ?? 0);
            NetRevenue = TotalRevenue - TotalDiscounts;
            AverageConsultation = TotalConsultations > 0 ? NetRevenue / TotalConsultations : 0;

            // Payment methods breakdown
            PaymentMethodsBreakdown = consultations
                .Where(c => !string.IsNullOrEmpty(c.PaymentMethod))
                .GroupBy(c => c.PaymentMethod!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0))
                );

            // Add "Non spécifié" for consultations without payment method
            var unspecifiedAmount = consultations
                .Where(c => string.IsNullOrEmpty(c.PaymentMethod))
                .Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0));
            
            if (unspecifiedAmount > 0)
            {
                PaymentMethodsBreakdown["Non spécifié"] = unspecifiedAmount;
            }

            // Services breakdown
            ServicesBreakdown = consultations
                .Where(c => c.ServiceId.HasValue || !string.IsNullOrEmpty(c.Service))
                .GroupBy(c => new
                {
                    c.ServiceId,
                    Name = c.ServiceEntity != null ? c.ServiceEntity.NomService : c.Service
                })
                .Select(g => new ServiceRevenueDto
                {
                    ServiceName = g.Key.Name ?? "Inconnu",
                    Count = g.Count(),
                    TotalRevenue = g.Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0))
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToList();

            // Daily revenue for charts
            DailyRevenue = consultations
                .GroupBy(c => c.DateConsultation!.Value.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0)),
                    ConsultationCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        private (DateTime start, DateTime end) GetDateRange()
        {
            var today = DateTime.Today;
            
            return Period switch
            {
                "today" => (today, today),
                "week" => (today.AddDays(-(((int)today.DayOfWeek + 6) % 7)), today),
                "month" => (new DateTime(today.Year, today.Month, 1), today),
                "year" => (new DateTime(today.Year, 1, 1), today),
                "custom" when StartDate.HasValue && EndDate.HasValue => (StartDate.Value, EndDate.Value),
                _ => (today, today)
            };
        }
    }

    public class ServiceRevenueDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int ConsultationCount { get; set; }
    }
}
