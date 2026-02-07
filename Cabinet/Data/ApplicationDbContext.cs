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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.ServiceEntity)
                .WithMany()
                .HasForeignKey(c => c.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasIndex(c => new { c.DateConsultation, c.Etat, c.PatientId });

            modelBuilder.Entity<Rendezvous>()
                .HasIndex(r => r.DateHeure);

            modelBuilder.Entity<Stock>()
                .HasIndex(s => s.CategoryId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(m => m.StockId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(m => m.EmployerId);
        }

        public override int SaveChanges()
        {
            ApplyAuditTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditTimestamps()
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    SetAuditValueIfPresent(entry, "CreatedAt", utcNow, onlyIfDefault: true);
                    SetAuditValueIfPresent(entry, "UpdatedAt", utcNow, onlyIfDefault: false);
                }
                else if (entry.State == EntityState.Modified)
                {
                    SetAuditValueIfPresent(entry, "UpdatedAt", utcNow, onlyIfDefault: false);

                    var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProperty != null)
                    {
                        createdAtProperty.IsModified = false;
                    }
                }
            }
        }

        private static void SetAuditValueIfPresent(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, DateTime value, bool onlyIfDefault)
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
            if (property == null)
            {
                return;
            }

            if (!onlyIfDefault || property.CurrentValue is null || (DateTime)property.CurrentValue == default)
            {
                property.CurrentValue = value;
            }
        }
    }
}
