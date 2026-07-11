using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("Ordonnances")]
    public class Ordonnance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("OrdonnanceID")]
        public int OrdonnanceID { get; set; }

        // FOREIGN KEY LINK
        [Column("PatientID")]
        public int PatientID { get; set; }

        [ForeignKey("PatientID")]
        public virtual Patient? Patient { get; set; }

        // FIXED: Date Object
        [Column("DatePrescription")]
        public DateTime DatePrescription { get; set; }

        // Links ordonnance directly to a consultation (replaces unreliable date-based matching)
        [Column("ConsultationID")]
        public int? ConsultationId { get; set; }

        [ForeignKey("ConsultationId")]
        public virtual Consultation? Consultation { get; set; }
    }
}