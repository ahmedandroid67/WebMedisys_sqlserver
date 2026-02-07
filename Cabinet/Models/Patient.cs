using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("patient")]
    public class Patient
    {
        [Key]
        [Column("id_patient")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Changed to Identity for auto-increment
        public int IdPatient { get; set; }

        [Column("nom")]
        [StringLength(255)]
        public string? Nom { get; set; }

        [Column("prenom")]
        [StringLength(255)]
        public string? Prenom { get; set; }

        [Column("cin")]
        [StringLength(50)]
        public string? Cin { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        // FIXED: Changed from String to DateTime
        [Column("date_naiss")]
        public DateTime? DateNaiss { get; set; }

        [Column("sexe")]
        [StringLength(10)]
        public string? Sexe { get; set; }

        [Column("adresse")]
        [StringLength(255)]
        public string? Adresse { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties (Links to other tables)
        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
        public virtual ICollection<Ordonnance> Ordonnances { get; set; } = new List<Ordonnance>();
    }
}
