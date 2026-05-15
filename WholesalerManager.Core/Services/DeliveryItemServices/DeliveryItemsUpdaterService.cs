using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryItemServices
{
    public class DeliveryItemsUpdaterService : IDeliveryItemsUpdaterService
    {
        private readonly IDeliveryItemsRepository _deliveryItemsRepository;
        private readonly IDeliveryItemsAdderService _deliveryItemsAdderService;

        public DeliveryItemsUpdaterService(IDeliveryItemsRepository deliveryItemsRepository, IDeliveryItemsAdderService deliveryItemsAdderService)
        {
            _deliveryItemsRepository = deliveryItemsRepository;
            _deliveryItemsAdderService = deliveryItemsAdderService;
        }

        public async Task<DeliveryItemResponse> UpdateDeliveryItem(DeliveryItemUpdateRequest? deliveryItemUpdateRequest)
        {
            if (deliveryItemUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(deliveryItemUpdateRequest));
            }

            ValidationHelper.ModelValidation(deliveryItemUpdateRequest);

            DeliveryItem item = deliveryItemUpdateRequest.ToDeliveryItem();

            DeliveryItem? changedItem = null;
            // Check if item is being added to an existing delivery
            if (item.DeliveryItemID == Guid.Empty)
            {
                changedItem = (await _deliveryItemsAdderService.AddDeliveryItem(item.ToDeliveryItemAddRequest())).ToDeliveryItem();
            }
            else
            {
                changedItem = await _deliveryItemsRepository.UpdateDeliveryItem(item);
            }

            if (changedItem is null)
            {
                throw new ArgumentException(nameof(changedItem));
            }

            return changedItem.ToDeliveryItemResponse();

        }

        public async Task<List<DeliveryItemResponse>> UpdateMultipleDeliveryItems(List<DeliveryItemUpdateRequest?>? deliveryItemUpdateRequests)
        {
            if (deliveryItemUpdateRequests is null)
            {
                throw new ArgumentNullException(nameof(deliveryItemUpdateRequests));
            }

            List<DeliveryItemResponse> updatedItems = new List<DeliveryItemResponse>() { };

            foreach(var item in deliveryItemUpdateRequests)
            {
                updatedItems.Add(await UpdateDeliveryItem(item));
            }

            return updatedItems;
        }
    }
}
