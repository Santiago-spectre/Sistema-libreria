namespace SistemaWebPapeleria.ViewModels
{
    public class ReturnRequestVM
    {
        public int SaleId { get; set; }
        public string? Reason { get; set; }
        public List<ReturnItemVM> Items { get; set; } = new();
    }

    public class ReturnItemVM
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
