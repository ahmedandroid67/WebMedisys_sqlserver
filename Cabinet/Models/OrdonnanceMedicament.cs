using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Required for [PrimaryKey]

namespace Cabinet.Models
{
    [Table("OrdonnanceMedicaments")]
    [PrimaryKey(nameof(OrdonnanceID), nameof(MedicamentID))] // Defines the composite key
    public class OrdonnanceMedicament
    {
        [Column("OrdonnanceID")]
        public int OrdonnanceID { get; set; }

        [Column("MedicamentID")]
        [StringLength(17)]
        public string MedicamentID { get; set; } = null!; // initialized to avoid warning

        [Column("quantite")]
        [StringLength(255)]
        public string? Quantite { get; set; }
    }
}