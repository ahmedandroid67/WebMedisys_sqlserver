using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("medicaments")]
    public class Medicament
    {
        // EF Core requires a Key. 
        // Since SQL 'float' maps to C# 'double', and your column allows NULLs, we use 'double?'.
        [Key]
        [Column("ID")]
        public double? Id { get; set; }

        [Column("CODE")]
        [StringLength(255)]
        public string? Code { get; set; }

        [Column("NOM")]
        [StringLength(255)]
        public string? Nom { get; set; }

        [Column("DCI1")]
        [StringLength(255)]
        public string? Dci1 { get; set; }

        [Column("DOSAGE1")]
        [StringLength(255)]
        public string? Dosage1 { get; set; }

        [Column("UNITE_DOSAGE1")]
        [StringLength(255)]
        public string? UniteDosage1 { get; set; }

        [Column("FORME")]
        [StringLength(255)]
        public string? Forme { get; set; }

        [Column("PRESENTATION")]
        [StringLength(255)]
        public string? Presentation { get; set; }

        [Column("PPV")]
        public double? Ppv { get; set; }

        [Column("PH")]
        public double? Ph { get; set; }

        [Column("PRIX_BR")]
        public double? PrixBr { get; set; }

        [Column("PRINCEPS_GENERIQUE")]
        [StringLength(255)]
        public string? PrincepsGenerique { get; set; }

        [Column("TAUX_REMBOURSEMENT")]
        [StringLength(255)]
        public string? TauxRemboursement { get; set; }
    }
}