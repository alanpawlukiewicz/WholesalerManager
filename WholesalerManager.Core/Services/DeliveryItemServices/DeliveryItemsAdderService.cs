using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryItemServices
{
    public class DeliveryItemsAdderService : IDeliveryItemsAdderService
    {
        private readonly IDeliveryItemsRepository _deliveryItemsRepository;
        private readonly ILogger<DeliveryItemsAdderService> _logger;

        public DeliveryItemsAdderService(IDeliveryItemsRepository deliveryItemsRepository, ILogger<DeliveryItemsAdderService> logger)
        {
            _deliveryItemsRepository = deliveryItemsRepository;
            _logger = logger;
        }

        public async Task<DeliveryItemResponse> AddDeliveryItem(DeliveryItemAddRequest? itemAddRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddDeliveryItem), nameof(DeliveryItemsAdderService));
            if (itemAddRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(itemAddRequest), nameof(AddDeliveryItem), nameof(DeliveryItemsAdderService));
                throw new ArgumentNullException(nameof(itemAddRequest));
            }

            ValidationHelper.ModelValidation(itemAddRequest);

            var item = itemAddRequest.ToDeliveryItem();
            item.DeliveryItemID = Guid.NewGuid();
            var addedItem = await _deliveryItemsRepository.AddDeliveryItem(item);

            return addedItem.ToDeliveryItemResponse();

        }

        public async Task<List<DeliveryItemResponse>> AddMultipleDeliveryItems(List<DeliveryItemAddRequest>? itemAddRequests)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddMultipleDeliveryItems), nameof(DeliveryItemsAdderService));
            if (itemAddRequests is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(itemAddRequests), nameof(AddMultipleDeliveryItems), nameof(DeliveryItemsAdderService));
                throw new ArgumentNullException(nameof(itemAddRequests));
            }

            List<DeliveryItemResponse> addedItems = new List<DeliveryItemResponse>() { };

            foreach (var item in itemAddRequests)
            {
                addedItems.Add(await AddDeliveryItem(item));
            }

            return addedItems;
        }
    }
}
