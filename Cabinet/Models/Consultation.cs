using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("Consultation")]
    public class Consultation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_consultation")]
        public int IdConsultation { get; set; }

        [Column("patient")]
        public int? PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        // NEW: Added Service column to track the specific medical act
        [Column("service")]
        [StringLength(100)]
        public string? Service { get; set; }

        [Column("prix_consul", TypeName = "decimal(18, 2)")]
        public decimal? PrixConsul { get; set; }

        [Column("remise", TypeName = "decimal(18, 2)")]
        public decimal? Remise { get; set; }

        // --- Medical Notes (Tab: Remarques) ---
        [Column("signe")]
        [StringLength(250)]
        public string? Signe { get; set; }

        [Column("diagnostique")]
        [StringLength(250)]
        public string? Diagnostique { get; set; }

        [Column("conduite")]
        [StringLength(250)]
        public string? Conduite { get; set; }


        // --- Vitals (Tab: Paramètres) ---
        [Column("t_gly")] public string? TGly { get; set; }
        [Column("t_tension")] public string? TTension { get; set; }
        [Column("t_poid")] public string? TPoid { get; set; }
        [Column("t_taille")] public string? TTaille { get; set; }
        [Column("t_spo")] public string? TSpo { get; set; }
        [Column("t_imc")] public string? TImc { get; set; }
        [Column("t_temp")] public string? TTemp { get; set; }
        [Column("t_fvc")] public string? TFvc { get; set; }
        [Column("t_fev")] public string? TFev { get; set; }
        [Column("t_ldl")] public string? TLdl { get; set; }

        // --- Workflow ---
        [Column("date_consultation")]
        public DateTime? DateConsultation { get; set; } = DateTime.Now;

        [Column("etat")]
        [StringLength(20)]
        public string? Etat { get; set; } = "Reception"; // Reception -> Visite -> Terminer

        [Column("mut_remplie")]
        public bool MutRemplie { get; set; }

        // --- Certificat d'Arrêt de Travail (CNSS) ---
        [Column("arret_debut")]
        public DateTime? ArretDebut { get; set; }

        [Column("arret_fin")]
        public DateTime? ArretFin { get; set; }

        [Column("arret_jours")]
        public int? ArretJours { get; set; }

        [Column("arret_motif")]
        [StringLength(500)]
        public string? ArretMotif { get; set; } // Motif médical (confidentiel)

        // --- Certificat Médical Général ---
        [Column("certificat_aptitude")]
        public bool? CertificatAptitude { get; set; } // true = apte, false = nécessite repos

        [Column("certificat_observation")]
        [StringLength(500)]
        public string? CertificatObservation { get; set; }

        // --- Payment Tracking ---
        [Column("payment_method")]
        [StringLength(50)]
        public string? PaymentMethod { get; set; } // Espèces, Carte, Chèque, Assurance

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Column("receipt_number")]
        [StringLength(50)]
        public string? ReceiptNumber { get; set; } // Auto-generated receipt number
    }
}