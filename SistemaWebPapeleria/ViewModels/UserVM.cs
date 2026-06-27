using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class UserVM : IValidatableObject
    {
        public int? UserId { get; set; }


        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "EL nombre no puede superar los 100 caracteres")]
        public string Name { get; set; }


        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "EL correo no tiene un formato válido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres")]
        public string Email { get; set; }


        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Password { get; set; }


        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public int RoleId { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Si es un usuario nuevo (sin UserId), el password es obligatorio
            if(UserId == null && string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(
                    "La contraseña es obligatoria para crear un usuario",
                    new[] { nameof(Password) });
            }
        }
    }
}
