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
        public Guid DeliveryID { get; set; }

        public Guid? SupplierID { get; set; }

        public DateTime? DeliveryDate { get; set; }

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
