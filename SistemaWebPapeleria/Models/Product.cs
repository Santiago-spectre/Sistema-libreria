using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebPapeleria.Models
{
    public class Product        //Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal SalePrice { get; set; }          //precio al cliente
        [Column(TypeName ="decimal(10,2)")]
        public decimal PurchasePrice { get; set; }      //precio del proveedor
        public int Stock {  get; set; }
        public int MinimumStock { get; set; }           //minimo del stock
        public bool IsService { get; set; }             //servicio o no
        public bool IsActive { get; set; }              //estado del producto en el sistema

        //Conexion con Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        //Conexion con Supplier
        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        
        //relacion: un producto aparece en muchos detallesVenta
        public ICollection<SaleDetail> SaleDetails { get; set; }

        public List<ReturnDetail> ReturnDetails { get; set; } = new();
    }
}
