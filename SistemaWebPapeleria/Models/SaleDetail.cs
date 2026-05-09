using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class SaleDetail     //DetalleVenta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleDetailId { get; set; }
        public int Quantity { get; set; }       //Cantidad de productos vendidos
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }          //precio unitario
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        //Conexion con Sale (venta)
        public int SaleId { get; set; }
        public Sale Sale { get; set; }

        //Conexion con Product (Producto)
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
