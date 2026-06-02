using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class User       //Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(100)]
        public string LastName { get; set; }
        [Required, MaxLength (100)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool Status { get; set; }                //estado 
        public int RoleId { get; set; }             //rol
        public Role Role { get; set; }

        //relacion: un usuario esta activo en el sistema
        public ICollection<Sale> Sales { get; set; }

        //relacion: un usuario realiza muchos cierres de caja
        public ICollection<CashClosing> CashClosings { get; set; }
    }
}
