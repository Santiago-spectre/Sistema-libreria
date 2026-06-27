using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class SupplierVM
    {
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Name { get; set; }


        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        [RegularExpression(@"^[0-9+\-\s]*$", ErrorMessage = "El teléfono solo puede contener números, espacios, + y -")]
        public string Phone { get; set; }


        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        public string Address { get; set; }


        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
        public string Description { get; set; }
    }
}
