namespace WholesalerManager.Core.DTO.ProductDTO
{
    public class ProductDeleteRequest
    {
        public Guid ProductID { get; set; }

        public string? ProductName { get; set; }

        public string SKU { get; set; } = "";

        public int StockQuantity { get; set; }
    }
}
