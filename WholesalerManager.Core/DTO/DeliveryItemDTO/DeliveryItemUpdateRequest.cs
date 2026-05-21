using System.ComponentModel.DataAnnotations;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Helpers;

namespace WholesalerManager.Core.DTO.DeliveryItemDTO
{
    public class DeliveryItemUpdateRequest
    {
        public Guid? DeliveryItemID { get; set; }

        [Required]
        public Guid? DeliveryID { get; set; }
        [Required(ErrorMessage = "Please select product connected to delivery.")]
        public Guid? ProductID { get; set; }

        public int Quantity { get; set; }

        [RegularExpression("^\\d+([.,]\\d{1,2})?$", ErrorMessage = "Unit price must be of money type.")]
        public string? PriceAtSale { get; set; }

        public DeliveryItem ToDeliveryItem()
        {
            return new DeliveryItem()
            {
                DeliveryItemID = DeliveryItemID ?? Guid.Empty,
                DeliveryID = DeliveryID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale.ToDecimalSafe(),
            };
        }
    }
}
