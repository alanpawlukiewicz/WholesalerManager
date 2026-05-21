using System.ComponentModel.DataAnnotations;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.OrderDTO
{
    public class OrderAddRequest
    {
        [Required(ErrorMessage = "Please select customer.")]
        public Guid? CustomerID { get; set; }

        [Required(ErrorMessage = "Please enter date of order.")]
        public DateTime? OrderDate { get; set; }

        public OrderStatus? Status { get; set; } = OrderStatus.PENDING;


        public Order ToOrder()
        {
            return new Order()
            {
                CustomerID = CustomerID,
                OrderDate = OrderDate,
                Status = Status?.ToString(),
            };
        }
    }
}
