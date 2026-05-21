using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

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

        public Order ToOrder()
        {
            return new Order()
            {
                OrderID = OrderID,
                CustomerID = CustomerID,
                OrderDate = OrderDate,
                Status = Status
            };
        }
        public OrderUpdateRequest ToOrderUpdateRequest()
        {
            return new OrderUpdateRequest()
            {
                OrderID = OrderID,
                CustomerID = CustomerID,
                OrderDate = OrderDate,
                Status = Enum.TryParse<OrderStatus>(Status, out var result) ? result : null,
            };
        }
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
