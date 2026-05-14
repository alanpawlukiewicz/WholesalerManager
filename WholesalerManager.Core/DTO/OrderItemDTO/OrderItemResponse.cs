using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.DTO.OrderItemDTO
{
    public class OrderItemResponse
    {
        public Guid OrderItemID { get; set; }
        public Guid? OrderID { get; set; }
        public Guid? ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtSale { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public decimal? ProductUnitPrice { get; set; }

        public OrderItem ToOrderItem()
        {
            return new OrderItem()
            {
                OrderItemID = OrderItemID,
                OrderID = OrderID,
                ProductID = ProductID,
                Quantity = Quantity,
                PriceAtSale = PriceAtSale
            };
        }
    }

    public static class OrderItemExtensions
    {
        /// <summary>
        /// Extension method converting a OrderItem entity to a OrderItemResponse DTO.
        /// </summary>
        /// <param name="item">The OrderItem entity to convert.</param>
        /// <returns>A OrderItemResponse DTO.</returns>
        public static OrderItemResponse ToOrderItemResponse(this OrderItem item)
        {
            return new OrderItemResponse()
            {
                OrderItemID = item.OrderItemID,
                OrderID = item.OrderID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                PriceAtSale = item.PriceAtSale,
                ProductName = item.Product?.ProductName,
                SKU = item.Product?.SKU,
                ProductUnitPrice = item.Product?.UnitPrice
                //Order = item.Order,
                //Product = item.Product
            };
        }

        public static OrderItemAddRequest ToOrderItemAddRequest(this OrderItem item)
        {
            return new OrderItemAddRequest()
            {
                OrderID = item.OrderID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                PriceAtSale = item.PriceAtSale.ToString(),
               
            };
        }

    }
}
