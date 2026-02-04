using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("rendezvous")]
    public class Rendezvous
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_rdv")]
        public int IdRdv { get; set; }

        [Column("nom")]
        [StringLength(25)]
        [Required(ErrorMessage = "Le nom est requis")]
        public string Nom { get; set; } = string.Empty;

        [Column("prenom")]
        [StringLength(25)]
        [Required(ErrorMessage = "Le prénom est requis")]
        public string Prenom { get; set; } = string.Empty;

        [Column("dateheure", TypeName = "datetime2")]
        [Required(ErrorMessage = "La date et l'heure sont requises")]
        public DateTime DateHeure { get; set; }

        [Column("service")]
        [StringLength(25)]
        [Required(ErrorMessage = "Le service est requis")]
        public string Service { get; set; } = string.Empty;

        // Optional: Add navigation property if you have foreign key
        // [ForeignKey("Service")]
        // public int? IdService { get; set; }
        // public virtual Service? ServiceNavigation { get; set; }

        [Column("sexe")]
        [StringLength(10)]
        public string? Sexe { get; set; }

        [Column("phone")]
        [StringLength(15)]
        [RegularExpression(@"^0[5-7]\d{8}$", ErrorMessage = "Format téléphone invalide (ex: 0612345678)")]
        public string? Phone { get; set; }
    }
}