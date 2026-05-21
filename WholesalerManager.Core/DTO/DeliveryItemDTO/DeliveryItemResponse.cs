using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.DeliveryItemDTO
{
    public class DeliveryItemResponse
    {
        public Guid DeliveryItemID { get; set; }
        public Guid? DeliveryID { get; set; }
        public Guid? ProductID { get; set; }

        public int Quantity { get; set; }

        public decimal PriceAtSale { get; set; }

        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public decimal? ProductUnitPrice { get; set; }

        //public Delivery? Delivery { get; set; }

        //public Product? Product { get; set; }

        public DeliveryItemUpdateRequest ToDeliveryItemUpdateRequest()
        {
            return new DeliveryItemUpdateRequest()
            {
                DeliveryItemID = DeliveryItemID,
                DeliveryID = DeliveryID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale.ToString()
            };
        }

        public DeliveryItemAddRequest ToDeliveryItemAddRequest()
        {
            return new DeliveryItemAddRequest()
            {
                DeliveryID = DeliveryID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale.ToString()
            };
        }

        public DeliveryItem ToDeliveryItem()
        {
            return new DeliveryItem()
            {
                DeliveryItemID = DeliveryItemID,
                DeliveryID = DeliveryID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale
            };
        }
    }

    public static class DeliveryItemExtensions
    {
        /// <summary>
        /// Extension method converting a DeliveryItem entity to a DeliveryItemResponse DTO.
        /// </summary>
        /// <param name="item">The DeliveryItem entity to convert.</param>
        /// <returns>A DeliveryItemResponse DTO.</returns>
        public static DeliveryItemResponse ToDeliveryItemResponse(this DeliveryItem item)
        {
            return new DeliveryItemResponse()
            {
                DeliveryItemID = item.DeliveryItemID,
                DeliveryID = item.DeliveryID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                PriceAtSale = item.PriceAtSale,
                ProductName = item.Product?.ProductName,
                SKU = item.Product?.SKU,
                ProductUnitPrice = item.Product?.UnitPrice
                //Delivery = item.Delivery,
                //Product = item.Product
            };
        }

        public static DeliveryItemAddRequest ToDeliveryItemAddRequest(this DeliveryItem item)
        {
            return new DeliveryItemAddRequest()
            {
                DeliveryID = item.DeliveryID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                PriceAtSale = item.PriceAtSale.ToString()
            };
        }
    }
}
