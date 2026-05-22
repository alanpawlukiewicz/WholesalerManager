using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveryUpdateCoordinatorService : IDeliveryUpdateCoordinatorService
    {
        private readonly IDeliveriesUpdaterService _deliveriesUpdaterService;
        private readonly IDeliveryItemsUpdaterService _deliveryItemsUpdaterService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeliveryUpdateCoordinatorService> _logger;

        public DeliveryUpdateCoordinatorService(IDeliveriesUpdaterService deliveriesUpdaterService, IDeliveryItemsUpdaterService deliveryItemsUpdaterService, IUnitOfWork unitOfWork, ILogger<DeliveryUpdateCoordinatorService> logger)
        {
            _deliveriesUpdaterService = deliveriesUpdaterService;
            _deliveryItemsUpdaterService = deliveryItemsUpdaterService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DeliveryResponse> UpdateFullDelivery(DeliveryUpdateRequest? deliveryUpdateRequest, List<DeliveryItemUpdateRequest>? items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateFullDelivery), nameof(DeliveryUpdateCoordinatorService));
            if (deliveryUpdateRequest is null || items is null)
            {
                _logger.LogError("{requestName} or {requestName2} from {methodName} from {serviceName} is null.", nameof(deliveryUpdateRequest), nameof(items), nameof(UpdateFullDelivery), nameof(DeliveryUpdateCoordinatorService));
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
            catch (Exception ex)
            {
                _logger.LogError("Exception caught from {methodName} from {serviceName}: {ex}.", nameof(UpdateFullDelivery), nameof(DeliveryUpdateCoordinatorService), ex);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }


        }
    }
}
