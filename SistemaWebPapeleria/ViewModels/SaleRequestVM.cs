using System.ComponentModel.DataAnnotations;

namespace SistemaWebPapeleria.ViewModels
{
    public class SaleRequestVM
    {
        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        [StringLength(30, ErrorMessage = "El método de pago no puede superar los 30 caracteres")]
        public string PaymentMethod { get; set; }


        [Range(0, 999999.99, ErrorMessage = "El descuento no puede ser negativo")]
        public decimal Discount { get; set; }


        public bool ReceiptIssued { get; set; }


        [Required(ErrorMessage = "Debe agregar al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
        public List<SaleItemVM> Items { get; set; }
    }
}
