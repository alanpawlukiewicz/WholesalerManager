using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Helpers;

namespace WholesalerManager.Core.DTO.OrderItemDTO
{
    public class OrderItemAddRequest
    {
        public Guid? OrderID { get; set; }

        [Required(ErrorMessage = "Please select product connected to order.")]
        public Guid? ProductID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger or equal {1}")]
        [NotNull]
        public int Quantity { get; set; }

        [RegularExpression("^\\d+([.,]\\d{1,2})?$", ErrorMessage = "Unit price must be of money type.")]
        public string? PriceAtSale { get; set; }

        public OrderItem ToOrderItem()
        {
            return new OrderItem()
            {
                OrderID = OrderID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale.ToDecimalSafe()
            };
        }
    }
}
