using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class SaleItemVM
    {
        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductId { get; set; }

        
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }


        [Range(0, 999999.99, ErrorMessage = "El precio unitario no puede ser negativo")]
        public decimal UnitPrice { get; set; }
    }
}
