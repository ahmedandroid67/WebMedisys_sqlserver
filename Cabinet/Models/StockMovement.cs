using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("stock_movements")]
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        public int StockId { get; set; }

        [ForeignKey("StockId")]
        public virtual Stock? Stock { get; set; }

        [Required]
        public int Quantite { get; set; }

        [Required]
        public string Type { get; set; } = "Entrée"; // "Entrée" or "Sortie"

        [Required]
        public string Motif { get; set; } = string.Empty;

        // New Fields
        [Required]
        public DateTime DateMouvement { get; set; } = DateTime.Now;

        public int EmployerId { get; set; }

        [ForeignKey("EmployerId")]
        public virtual Employer? Employer { get; set; }
    }
}