using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryItemServices
{
    public class DeliveryItemsAdderService : IDeliveryItemsAdderService
    {
        private readonly IDeliveryItemsRepository _deliveryItemsRepository;

        public DeliveryItemsAdderService(IDeliveryItemsRepository deliveryItemsRepository)
        {
            _deliveryItemsRepository = deliveryItemsRepository;
        }

        public async Task<DeliveryItemResponse> AddDeliveryItem(DeliveryItemAddRequest? itemAddRequest)
        {
            if (itemAddRequest is null)
            {
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
            if (itemAddRequests is null)
            {
                throw new ArgumentNullException(nameof(itemAddRequests));
            }

            foreach (var item in itemAddRequests)
            {
                ValidationHelper.ModelValidation(item);
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
