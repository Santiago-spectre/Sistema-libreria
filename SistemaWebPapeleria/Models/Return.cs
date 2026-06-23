using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class Return
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReturnId { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(300)]
        public string? Reason { get; set; }

        // FK Sale
        public int SaleId { get; set; }
        public Sale Sale { get; set; }

        // FK User
        public int UserId { get; set; }
        public User User { get; set; }

        public List<ReturnDetail> ReturnDetails { get; set; } = new();
    }
}