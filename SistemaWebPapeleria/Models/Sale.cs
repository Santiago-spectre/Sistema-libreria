using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class Sale       //venta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleId { get; set; }
        public DateTime Date {  get; set; }     //fecha y hora

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total {  get; set; }     //total vinal de venta
        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; }       //metodo de pago
        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; }           //descuento, opcional
        public bool ReceiptIssued { get; set; }          //indica comprobante

        //Conexion con User - que vendedor registro la venta
        public int UserId { get; set; }
        public User User { get; set; }

        //Relacion: una venta tiene muchos detalles
        public ICollection<SaleDetail> SaleDetails { get; set; }

        //relacion: una venta puede tener un comprobante
        public Receipt Receipt { get; set; }

    }
}
