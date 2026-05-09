using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }
        [Required, MaxLength(80)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }

        //relacion: una categoria tiene muchos productos
        public ICollection<Product> Products { get; set; }
    }
}
