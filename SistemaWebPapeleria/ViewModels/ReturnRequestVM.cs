using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class ReturnRequestVM
    {
        [Required(ErrorMessage = "Debe indicar la venta a la que pertenece la devolución")]
        public int SaleId { get; set; }


        [StringLength(300, ErrorMessage = "El motivo no puede superar los 300 caracteres")]
        public string? Reason { get; set; }


        [Required(ErrorMessage = "Debe agregar al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
        public List<ReturnItemVM> Items { get; set; } = new();
    }

    public class ReturnItemVM
    {
        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductId { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "La cantidad a devolver debe ser mayor a 0")]
        public int Quantity { get; set; }
    }
}
