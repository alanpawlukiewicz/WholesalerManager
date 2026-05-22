using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryItemServices
{
    public class DeliveryItemsUpdaterService : IDeliveryItemsUpdaterService
    {
        private readonly IDeliveryItemsRepository _deliveryItemsRepository;
        private readonly IDeliveryItemsAdderService _deliveryItemsAdderService;
        private readonly ILogger<DeliveryItemsUpdaterService> _logger;

        public DeliveryItemsUpdaterService(IDeliveryItemsRepository deliveryItemsRepository, IDeliveryItemsAdderService deliveryItemsAdderService, ILogger<DeliveryItemsUpdaterService> logger)
        {
            _deliveryItemsRepository = deliveryItemsRepository;
            _deliveryItemsAdderService = deliveryItemsAdderService;
            _logger = logger;
        }

        public async Task<DeliveryItemResponse> UpdateDeliveryItem(DeliveryItemUpdateRequest? deliveryItemUpdateRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateDeliveryItem), nameof(DeliveryItemsUpdaterService));

            if (deliveryItemUpdateRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryItemUpdateRequest), nameof(UpdateDeliveryItem), nameof(DeliveryItemsUpdaterService));
                throw new ArgumentNullException(nameof(deliveryItemUpdateRequest));
            }

            ValidationHelper.ModelValidation(deliveryItemUpdateRequest);

            DeliveryItem item = deliveryItemUpdateRequest.ToDeliveryItem();

            DeliveryItem? changedItem = null;
            // Check if item is being added to an existing delivery
            if (item.DeliveryItemID == Guid.Empty)
            {
                _logger.LogInformation("Adding delivery item to database");
                changedItem = (await _deliveryItemsAdderService.AddDeliveryItem(item.ToDeliveryItemAddRequest())).ToDeliveryItem();
            }
            else
            {
                _logger.LogInformation("Updating delivery item.");
                changedItem = await _deliveryItemsRepository.UpdateDeliveryItem(item);
            }

            if (changedItem is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(changedItem), nameof(UpdateDeliveryItem), nameof(DeliveryItemsUpdaterService));
                throw new ArgumentException(nameof(changedItem));
            }

            return changedItem.ToDeliveryItemResponse();

        }

        public async Task<List<DeliveryItemResponse>?> UpdateMultipleDeliveryItems(List<DeliveryItemUpdateRequest?>? deliveryItemUpdateRequests)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateMultipleDeliveryItems), nameof(DeliveryItemsUpdaterService));

            if (deliveryItemUpdateRequests is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryItemUpdateRequests), nameof(UpdateMultipleDeliveryItems), nameof(DeliveryItemsUpdaterService));
                throw new ArgumentNullException(nameof(deliveryItemUpdateRequests));
            }

            List<DeliveryItemResponse> updatedItems = new List<DeliveryItemResponse>() { };

            foreach (var item in deliveryItemUpdateRequests)
            {
                updatedItems.Add(await UpdateDeliveryItem(item));
            }

            return updatedItems;
        }
    }
}
