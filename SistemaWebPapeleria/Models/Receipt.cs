using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class Receipt            //Comprobante
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReceiptId { get; set; }

        public DateTime IssueDate { get; set; }             // Fecha y hora en que se emitió el comprobante

        [MaxLength(300)]
        public string Observations { get; set; }            // Observaciones adicionales, opcional

        // Conexión con Sale (FK) — a qué venta pertenece este comprobante (1:1)
        public int SaleId { get; set; }
        public Sale Sale { get; set; }
    }
}
