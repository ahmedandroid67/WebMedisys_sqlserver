using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet.Models
{
    [Table("Employer")]
    public class Employer
    {
        [Key]
        public int IdEmployer { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        [Column("mot_passe")]
        public string MotPasse { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
        [Compare("MotPasse", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [DataType(DataType.Password)]
        public string ConfirmMotPasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez sélectionner un rôle.")]
        public string Role { get; set; } = string.Empty;

        public string? Fonction { get; set; }
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
    }
}