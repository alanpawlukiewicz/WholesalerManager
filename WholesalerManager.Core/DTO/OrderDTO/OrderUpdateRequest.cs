using System.ComponentModel.DataAnnotations;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.OrderDTO
{
    public class OrderUpdateRequest
    {
        [Required]
        public Guid OrderID { get; set; }
        [Required(ErrorMessage = "Please select customer.")]
        public Guid? CustomerID { get; set; }

        [Required(ErrorMessage = "Please enter date of order.")]
        public DateTime? OrderDate { get; set; }

        [Required(ErrorMessage = "Order must have a status.")]
        public OrderStatus? Status { get; set; }


        public Order ToOrder()
        {
            return new Order()
            {
                OrderID = OrderID,
                CustomerID = CustomerID,
                OrderDate = OrderDate,
                Status = Status.ToString(),
            };
        }
    }
}
