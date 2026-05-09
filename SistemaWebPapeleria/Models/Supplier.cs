using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class Supplier   //Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierId { get; set; }         //IdProveedor
        [Required, MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(20)]
        public string Phone {  get; set; }
        [MaxLength(200)]
        public string Address { get; set; }         //Direccion
        [MaxLength(200)]
        public string Description { get; set; }    
        public bool Status { get; set; }    //estado: activo o desactivo

        // Relación: un proveedor puede dar muchos productos
        public ICollection<Product> Products { get; set; }
    }
}
