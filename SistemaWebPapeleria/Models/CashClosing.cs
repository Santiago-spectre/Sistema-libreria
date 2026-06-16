using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class CashClosing        //Cierre de caja
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashClosingId { get; set; }
        public DateTime Date {  get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal InitialAmount { get; set; }          //monto inicial
        [Column(TypeName = "decimal(10,2)")]                
        public decimal TotalCash { get; set; }              //total recaudado en efectivo

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalYape { get; set; }              // Total recaudado por Yape

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPlin { get; set; }              // Total recaudado por Plin

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCard { get; set; }              // Total recaudado por tarjeta

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalSales { get; set; }             // Suma total de todas las ventas del día

        [Column(TypeName = "decimal(10,2)")]
        public decimal ClosingAmount { get; set; }          // Monto final al cerrar la caja

        public bool IsOpen { get; set; }                    //Caja abierta o cerrada

        // Conexión con User (FK) — quién realizó el cierre
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
