using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Helpers;

namespace WholesalerManager.Core.DTO.OrderItemDTO
{
    public class OrderItemAddRequest
    {
        //public Guid OrderItemID { get; set; }
        [Required(ErrorMessage = "Please select order.")]
        public Guid? OrderID { get; set; }

        [Required(ErrorMessage = "Please select product connected to order.")]
        public Guid? ProductID { get; set; }
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
