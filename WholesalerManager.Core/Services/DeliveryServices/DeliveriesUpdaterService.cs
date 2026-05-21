using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesUpdaterService : IDeliveriesUpdaterService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;

        public DeliveriesUpdaterService(IDeliveriesRepository deliveriesRepository)
        {
            _deliveriesRepository = deliveriesRepository;
        }

        public async Task<bool> SetDeliveryAsReceived(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                return false;
            }
            var matchingDelivery = await _deliveriesRepository.GetDeliveryById(orderID);

            if (matchingDelivery == null || matchingDelivery.Status != DeliveryStatus.IN_TRANSIT.ToString())
            {
                return false;
            }

            matchingDelivery.Status = DeliveryStatus.RECEIVED.ToString();

            await _deliveriesRepository.Save();

            return true;

        }

        public async Task<DeliveryResponse> UpdateDelivery(DeliveryUpdateRequest? deliveryUpdateRequest)
        {
            if (deliveryUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(deliveryUpdateRequest));
            }

            ValidationHelper.ModelValidation(deliveryUpdateRequest);

            Delivery delivery = deliveryUpdateRequest.ToDelivery();
            Delivery? updatedDelivery = await _deliveriesRepository.UpdateDelivery(delivery);

            if (updatedDelivery is null)
            {
                throw new ArgumentException(nameof(updatedDelivery));
            }

            return updatedDelivery.ToDeliveryResponse();
        }
    }
}
