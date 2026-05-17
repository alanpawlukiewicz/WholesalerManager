using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveryRegistrationService : IDeliveryRegistrationService
    {
        private readonly IDeliveriesAdderService _deliveriesAdderService;
        private readonly IDeliveryItemsAdderService _deliveryItemsAdderService;
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryRegistrationService(IDeliveriesAdderService deliveriesAdderService, IDeliveryItemsAdderService deliveryItemsAdderService, IUnitOfWork unitOfWork)
        {
            _deliveriesAdderService = deliveriesAdderService;
            _deliveryItemsAdderService = deliveryItemsAdderService;
            _unitOfWork = unitOfWork;
        }
        public async Task<DeliveryResponse> RegisterFullDelivery(DeliveryAddRequest? deliveryAddRequest, List<DeliveryItemAddRequest>? items)
        {
            if (deliveryAddRequest is null || items is null)
            {
                throw new ArgumentNullException(nameof(deliveryAddRequest));
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var delivery = await _deliveriesAdderService.AddDelivery(deliveryAddRequest);

                items.ForEach(i => i.DeliveryID = delivery.DeliveryID);
                await _deliveryItemsAdderService.AddMultipleDeliveryItems(items);

                await _unitOfWork.CommitTransactionAsync();

                return delivery;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
