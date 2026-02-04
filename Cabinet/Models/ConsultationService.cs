using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("consultation_service")]
    public class ConsultationService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_link")]
        public int IdLink { get; set; }

        [Column("id_consultation")]
        public int? IdConsultation { get; set; }

        [Column("id_service")]
        public int? IdService { get; set; }
    }
}