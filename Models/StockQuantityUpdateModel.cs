namespace EbillingV2.Models
{
    public class StockQuantityUpdateModel
    {
        public int Id { get; set; }
        public int StockQuantity { get; set; }

        public string? Active { get; set; }
    }
}
