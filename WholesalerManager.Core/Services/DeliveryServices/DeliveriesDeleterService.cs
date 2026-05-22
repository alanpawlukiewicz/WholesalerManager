using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesDeleterService : IDeliveriesDeleterService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;
        private readonly ILogger<DeliveriesDeleterService> _logger;

        public DeliveriesDeleterService(IDeliveriesRepository deliveriesRepository, ILogger<DeliveriesDeleterService> logger)
        {
            _deliveriesRepository = deliveriesRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteDeliveryByID(Guid? deliveryID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteDeliveryByID), nameof(DeliveriesDeleterService));

            if (deliveryID is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryID), nameof(DeleteDeliveryByID), nameof(DeliveriesDeleterService));
                throw new ArgumentNullException(nameof(deliveryID));
            }

            if (deliveryID == Guid.Empty)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is empty.", nameof(deliveryID), nameof(DeleteDeliveryByID), nameof(DeliveriesDeleterService));
                throw new ArgumentException(nameof(deliveryID));
            }

            return await _deliveriesRepository.DeleteDeliveryById(deliveryID.Value);
        }
    }
}
