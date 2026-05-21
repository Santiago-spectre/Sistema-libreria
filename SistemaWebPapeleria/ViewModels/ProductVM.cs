namespace SistemaWebPapeleria.ViewModels
{
    public class ProductVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal SalePrice { get; set; }      //precio de venta al cliente
        public decimal PurchasePrice { get; set; }  //precio de compra al proveedor
        public int Stock { get; set; }
        public int MinimumStock { get; set; } = 5;
        public bool IsService { get; set; }         //Si es servicio o no(copias, impresiones)
        public bool IsActive { get; set; } = true;  //estado activo/inactivo
        public int CategoryId { get; set; }
        public int? SupplierId { get; set; }
    }
}
