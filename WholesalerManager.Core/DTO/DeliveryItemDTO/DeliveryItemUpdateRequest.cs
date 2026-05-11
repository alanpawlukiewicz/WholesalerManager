using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.DeliveryItemDTO
{
    public class DeliveryItemUpdateRequest
    {
        [Required()]
        public Guid DeliveryItemID { get; set; }

        [Required(ErrorMessage = "Please select delivery.")]
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
                DeliveryItemID = DeliveryItemID,
                DeliveryID = DeliveryID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale is not null ? decimal.Parse(PriceAtSale.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture) : 0,
            };
        }
    }
}
