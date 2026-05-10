using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryItemServices
{
    public class DeliveryItemsGetterService : IDeliveryItemsGetterService
    {
        private readonly IDeliveryItemsRepository _deliveryItemsRepository;

        public DeliveryItemsGetterService(IDeliveryItemsRepository deliveryItemsRepository)
        {
            _deliveryItemsRepository = deliveryItemsRepository;
        }

        public async Task<List<DeliveryItemResponse>> GetAllDeliveryItems()
        {
            var items = await _deliveryItemsRepository.GetAllDeliveryItems();
            return items.Select(i => i.ToDeliveryItemResponse()).ToList();
        }

        public async Task<List<DeliveryItemResponse>?> GetAllDeliveryItemsFromDelivery(Guid? deliveryID)
        {
            if (deliveryID is null)
            {
                return null;
            }
            var foundItems = await _deliveryItemsRepository.GetAllDeliveryItemsFromDelivery(deliveryID.Value);
            return foundItems.Select(i => i.ToDeliveryItemResponse()).ToList();
        }
    }
}
