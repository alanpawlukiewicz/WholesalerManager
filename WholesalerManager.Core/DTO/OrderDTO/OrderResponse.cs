using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.DTO.OrderDTO
{
    public class OrderResponse
    {
        public Guid OrderID { get; set; }
        public Guid? CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Status { get; set; }
        public string? CustomerName { get; set; }
        public string? TIN { get; set; }
    }

    public static class OrderExtensions
    {
        public static OrderResponse ToOrderResponse(this Order order)
        {
            return new OrderResponse()
            {
                OrderID = order.OrderID,
                CustomerID = order.CustomerID,
                CustomerName = order.Customer?.CustomerName,
                TIN = order.Customer?.TIN,
                OrderDate = order.OrderDate,
                Status = order.Status
            };
        }
    }
}
