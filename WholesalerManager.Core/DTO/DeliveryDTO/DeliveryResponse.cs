using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.DeliveryDTO
{
    public class DeliveryResponse
    {
        public Guid DeliveryID { get; set; }

        public Guid? SupplierID { get; set; }

        public string? SupplierName { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? Status { get; set; }
    }

    public static class DeliveryExtensions
    {
        /// <summary>
        /// Extension method converting a Delivery entity to a DeliveryResponse DTO.
        /// </summary>
        /// <param name="delivery">The Delivery entity to convert.</param>
        /// <returns>A DeliveryResponse DTO.</returns>
        public static DeliveryResponse ToDeliveryResponse(this Delivery delivery)
        {
            return new DeliveryResponse()
            {
                DeliveryID = delivery.DeliveryID,
                SupplierID = delivery.SupplierID,
                SupplierName = delivery.Supplier?.SupplierName,
                DeliveryDate = delivery.DeliveryDate,
                Status = delivery.Status
            };
        }
    }
}
