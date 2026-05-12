using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.DeliveryDTO
{
    public class DeliveryUpdateRequest
    {
        [Required()]
        public Guid DeliveryID { get; set; }

        public Guid? SupplierID { get; set; }

        [Required(ErrorMessage = "Please enter valid date.")]
        public DateTime? DeliveryDate { get; set; }

        [Required(ErrorMessage = "Delivery must have a status.")]
        public DeliveryStatus? Status { get; set; }

        public Delivery ToDelivery()
        {
            return new Delivery()
            {
                DeliveryID = DeliveryID,
                SupplierID = SupplierID,
                DeliveryDate = DeliveryDate,
                Status = Status.ToString()
            };
        }

    }
}
