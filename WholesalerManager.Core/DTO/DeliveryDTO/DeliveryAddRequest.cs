using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
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
                DeliveryDate = OrderDate,
                Status = Status.ToString()
            };
        }
    }
}
