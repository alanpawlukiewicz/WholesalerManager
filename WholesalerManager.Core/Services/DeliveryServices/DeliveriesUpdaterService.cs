using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DeliveriesUpdaterService> _logger;

        public DeliveriesUpdaterService(IDeliveriesRepository deliveriesRepository, ILogger<DeliveriesUpdaterService> logger)
        {
            _deliveriesRepository = deliveriesRepository;
            _logger = logger;
        }

        public async Task<bool> SetDeliveryAsReceived(Guid orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(SetDeliveryAsReceived), nameof(DeliveriesUpdaterService));
            if (orderID == Guid.Empty)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(orderID), nameof(SetDeliveryAsReceived), nameof(DeliveriesUpdaterService));
                throw new ArgumentNullException(nameof(orderID));
            }
            var matchingDelivery = await _deliveriesRepository.GetDeliveryById(orderID);

            if (matchingDelivery == null)
            {
                _logger.LogWarning("Delivery not found.");
                return false;
            }

            if (matchingDelivery.Status != DeliveryStatus.IN_TRANSIT.ToString())
            {
                _logger.LogWarning("Invalid delivery status: {status}, expected: {status2}", matchingDelivery.Status, nameof(DeliveryStatus.IN_TRANSIT));
                return false;
            }

            matchingDelivery.Status = DeliveryStatus.RECEIVED.ToString();

            await _deliveriesRepository.Save();

            return true;

        }

        public async Task<DeliveryResponse> UpdateDelivery(DeliveryUpdateRequest? deliveryUpdateRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateDelivery), nameof(DeliveriesUpdaterService));
            if (deliveryUpdateRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryUpdateRequest), nameof(UpdateDelivery), nameof(DeliveriesUpdaterService));
                throw new ArgumentNullException(nameof(deliveryUpdateRequest));
            }

            ValidationHelper.ModelValidation(deliveryUpdateRequest);

            Delivery delivery = deliveryUpdateRequest.ToDelivery();
            Delivery? updatedDelivery = await _deliveriesRepository.UpdateDelivery(delivery);

            if (updatedDelivery is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(updatedDelivery), nameof(updatedDelivery), nameof(DeliveriesUpdaterService));
                throw new ArgumentException(nameof(updatedDelivery));
            }

            return updatedDelivery.ToDeliveryResponse();
        }
    }
}
