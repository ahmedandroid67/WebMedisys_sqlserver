using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("category_stock")]
    public class CategoryStock
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est requis")]
        public string Nom { get; set; } = string.Empty; // e.g., "Consommables", "Outils", "Médicaments"

        public string? Icone { get; set; } // To store Font Awesome class names

        public virtual ICollection<Stock>? Produits { get; set; }
    }

    [Table("stock")]
    public class Stock
    {
        [Key]
        [Column("id_produit")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du produit est requis")]
        [Column("nom_produit")]
        [StringLength(30)]
        public string Nom { get; set; } = string.Empty;

        [Column("obs")]
        [StringLength(50)]
        public string? Observation { get; set; }

        [Column("quantite")]
        public int Quantite { get; set; }

        [Column("alarme")]
        public int Alarme { get; set; } // Trigger for low stock alerts

        // Foreign Key for Categories
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CategoryStock? Category { get; set; }
    }
}