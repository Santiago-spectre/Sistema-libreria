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
        public List<Sale> LastSales { get; set; }           //ultimas ventas
    }
}
