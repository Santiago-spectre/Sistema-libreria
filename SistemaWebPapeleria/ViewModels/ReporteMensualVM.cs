using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.ViewModels
{
    public class ReporteMensualVM
    {
        public string MonthName { get; set; }
        public int Year { get; set; }
        public List<Sale> Sales { get; set; } = new(); 
    }
}
