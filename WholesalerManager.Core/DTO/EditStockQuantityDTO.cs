namespace WholesalerManager.Core.DTO
{
    public class EditStockQuantityDTO
    {
        public Guid ProductID { get; set; }
        public int NewStockQuantity { get; set; }
    }
}
