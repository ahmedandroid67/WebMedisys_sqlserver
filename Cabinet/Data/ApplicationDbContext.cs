using Cabinet.Models;
using Microsoft.EntityFrameworkCore;

namespace Cabinet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Employer> Employer { get; set; }
        public DbSet<Consultation> Consultation { get; set; }
        public DbSet<ConsultationService> ConsultationService { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Medicament> Medicament { get; set; }
        public DbSet<Ordonnance> Ordonnance { get; set; }
        public DbSet<OrdonnanceMedicament> OrdonnanceMedicament { get; set; }
        public DbSet<Rendezvous> Rendezvous { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<CategoryStock> CategoryStocks { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<CabinetInfo> CabinetInfo { get; set; }

    }
}