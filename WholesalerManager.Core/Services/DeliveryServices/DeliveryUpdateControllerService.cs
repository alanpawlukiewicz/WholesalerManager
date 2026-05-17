using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.Services.DeliveryItemServices;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveryUpdateControllerService : IDeliveryUpdateControllerService
    {
        private readonly IDeliveriesUpdaterService _deliveriesUpdaterService;
        private readonly IDeliveryItemsUpdaterService _deliveryItemsUpdaterService;
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryUpdateControllerService(IDeliveriesUpdaterService deliveriesUpdaterService, IDeliveryItemsUpdaterService deliveryItemsUpdaterService, IUnitOfWork unitOfWork)
        {
            _deliveriesUpdaterService = deliveriesUpdaterService;
            _deliveryItemsUpdaterService = deliveryItemsUpdaterService;
            _unitOfWork = unitOfWork;
        }

        public async Task<DeliveryResponse> UpdateFullDelivery(DeliveryUpdateRequest? deliveryUpdateRequest, List<DeliveryItemUpdateRequest>? items)
        {
            if (deliveryUpdateRequest is null || items is null)
            {
                throw new ArgumentNullException(nameof(deliveryUpdateRequest));
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var delivery = await _deliveriesUpdaterService.UpdateDelivery(deliveryUpdateRequest);
                await _deliveryItemsUpdaterService.UpdateMultipleDeliveryItems(items!);

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
