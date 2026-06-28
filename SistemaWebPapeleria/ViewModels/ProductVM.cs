using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class ProductVM : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre es Obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
        public string Name { get; set; }


        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres")]
        public string? Description { get; set; }


        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio de venta debe ser mayor a 0")]
        public decimal SalePrice { get; set; }      //precio de venta al cliente


        public decimal? PurchasePrice { get; set; }  //precio de compra al proveedor


        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int? Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int? MinimumStock { get; set; } = 5;


        public bool IsService { get; set; }         //Si es servicio o no(copias, impresiones)
        public bool IsActive { get; set; } = true;  //estado activo/inactivo


        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public int CategoryId { get; set; }
        public int? SupplierId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsService && SupplierId == null)
            {
                yield return new ValidationResult(
                    "Debe seleccionar un proveedor para productos fisicos",
                    new[] { nameof(SupplierId) });
            }

            if (!IsService && (PurchasePrice == null || PurchasePrice <= 0))
            {
                yield return new ValidationResult(
                    "El precio de compra es obligatorio para productos fisicos",
                    new[] { nameof(PurchasePrice) });
            }
        }
    }
}
