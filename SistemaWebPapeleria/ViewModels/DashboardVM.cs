using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.ViewModels
{
    public class DashboardVM
    {
        public string UserRole { get; set; }
        public string UserName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalSales { get; set; }
        public int TotalUsers { get; set; }
        public decimal TodaySales { get; set; }         //ventas del dia
        public List<Sale> LastSales { get; set; }           // ultimas ventas
        
        //ventas de la ultima semana
        public List<string> WeekDays { get; set; }
        public List<decimal> WeekSales { get; set; }

        //ventas por metodo de pago
        public decimal SalesEfectivo { get; set; }
        public decimal SalesYape { get; set; }
        public decimal SalesPlin { get; set; }
        public decimal SalesTarjeta { get; set; }

        //Productos con stock bajo
        public List<Product> LowStockProducts { get; set; }

        // top 5 productos mas vendidos
        public List<TopProductVM> TopProducts { get; set; }
    }

    public class TopProductVM
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
    }
}
