using System.ComponentModel.DataAnnotations;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.DeliveryDTO
{
    public class DeliveryAddRequest
    {
        [Required(ErrorMessage = "Please select supplier.")]
        public Guid? SupplierID { get; set; }

        [Required(ErrorMessage = "Please enter date of order.")]
        public DateTime? OrderDate { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.ORDERED;


        public Delivery ToDelivery()
        {
            return new Delivery()
            {
                SupplierID = SupplierID,
                OrderDate = OrderDate,
                Status = Status.ToString()
            };
        }
    }
}
