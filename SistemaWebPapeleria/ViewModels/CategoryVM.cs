using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class CategoryVM
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres")]
        public string Name { get; set; }


        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
        public string? Description { get; set; }
    }
}
