using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

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

            var addedItems = await _deliveryItemsRepository.AddMultipleDeliveryItems(itemAddRequests.Select(i => i.ToDeliveryItem()).ToList());

            return addedItems.Select(i => i.ToDeliveryItemResponse()).ToList();
        }
    }
}
