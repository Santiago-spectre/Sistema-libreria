using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class ReturnDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReturnDetailId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        // FK Return
        public int ReturnId { get; set; }
        public Return Return { get; set; }

        // FK Product
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}