using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.DeliveryDTO
{
    public class DeliveryResponse
    {
        public Guid DeliveryID { get; set; }

        public Guid? SupplierID { get; set; }

        public string? SupplierName { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? Status { get; set; }

        public Delivery ToDelivery()
        {
            return new Delivery()
            {
                DeliveryID = DeliveryID,
                SupplierID = SupplierID,
                OrderDate = OrderDate,
                Status = Status
            };
        }
        public DeliveryUpdateRequest ToDeliveryUpdateRequest()
        {
            return new DeliveryUpdateRequest()
            {
                DeliveryID = DeliveryID,
                SupplierID = SupplierID,
                OrderDate = OrderDate,
                Status = Enum.TryParse<DeliveryStatus>(Status, out var result) ? result : null
            };
        }
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
                OrderDate = delivery.OrderDate,
                Status = delivery.Status
            };
        }
    }
}
