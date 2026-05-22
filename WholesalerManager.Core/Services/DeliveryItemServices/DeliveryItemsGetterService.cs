using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryItemServices
{
    public class DeliveryItemsGetterService : IDeliveryItemsGetterService
    {
        private readonly IDeliveryItemsRepository _deliveryItemsRepository;
        private readonly ILogger<DeliveryItemsGetterService> _logger;

        public DeliveryItemsGetterService(IDeliveryItemsRepository deliveryItemsRepository, ILogger<DeliveryItemsGetterService> logger)
        {
            _deliveryItemsRepository = deliveryItemsRepository;
            _logger = logger;
        }

        public async Task<List<DeliveryItemResponse>> GetAllDeliveryItems()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllDeliveryItems), nameof(DeliveryItemsGetterService));

            var items = await _deliveryItemsRepository.GetAllDeliveryItems();
            return items.Select(i => i.ToDeliveryItemResponse()).ToList();
        }

        public async Task<List<DeliveryItemResponse>?> GetAllDeliveryItemsFromDelivery(Guid? deliveryID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllDeliveryItemsFromDelivery), nameof(DeliveryItemsGetterService));

            if (deliveryID is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryID), nameof(GetAllDeliveryItemsFromDelivery), nameof(DeliveryItemsGetterService));
                throw new ArgumentNullException(nameof(deliveryID));
            }
            var foundItems = await _deliveryItemsRepository.GetAllDeliveryItemsFromDelivery(deliveryID.Value);
            return foundItems.Select(i => i.ToDeliveryItemResponse()).ToList();
        }
    }
}
