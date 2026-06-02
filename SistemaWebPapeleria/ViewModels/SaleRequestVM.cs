namespace SistemaWebPapeleria.ViewModels
{
    public class SaleRequestVM
    {
        public string PaymentMethod { get; set; }
        public decimal Discount { get; set; }
        public bool ReceiptIssued { get; set; }
        public List<SaleItemVM> Items { get; set; }
    }
}
