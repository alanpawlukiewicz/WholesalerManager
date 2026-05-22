using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DeliveryRegistrationService> _logger;

        public DeliveryRegistrationService(IDeliveriesAdderService deliveriesAdderService, IDeliveryItemsAdderService deliveryItemsAdderService, IUnitOfWork unitOfWork, ILogger<DeliveryRegistrationService> logger)
        {
            _deliveriesAdderService = deliveriesAdderService;
            _deliveryItemsAdderService = deliveryItemsAdderService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<DeliveryResponse> RegisterFullDelivery(DeliveryAddRequest? deliveryAddRequest, List<DeliveryItemAddRequest>? items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(RegisterFullDelivery), nameof(DeliveryRegistrationService));

            if (deliveryAddRequest is null || items is null)
            {
                _logger.LogError("{requestName} or {requestName2} from {methodName} from {serviceName} is null.", nameof(deliveryAddRequest), nameof(items), nameof(RegisterFullDelivery), nameof(DeliveryRegistrationService));
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
            catch (Exception ex)
            {
                _logger.LogError("Exception caught from {methodName} from {serviceName}: {ex}.", nameof(RegisterFullDelivery), nameof(DeliveryRegistrationService), ex);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
