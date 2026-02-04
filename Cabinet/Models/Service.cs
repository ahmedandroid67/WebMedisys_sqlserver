using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("services")]
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_service")]
        public int IdService { get; set; }

        [Column("service")]
        [StringLength(100)]
        public string? NomService { get; set; }

        // FIXED: Added TypeName to keep 2 decimal places
        [Column("prix", TypeName = "decimal(18, 2)")]
        public decimal? Prix { get; set; }

        [Column("obs")]
        [StringLength(250)]
        public string? Obs { get; set; }
    }
}