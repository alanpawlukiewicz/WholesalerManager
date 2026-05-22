using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesAdderService : IDeliveriesAdderService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;
        private readonly ISuppliersGetterService _suppliersGetterService;
        private readonly ILogger<DeliveriesAdderService> _logger;

        public DeliveriesAdderService(IDeliveriesRepository deliveriesRepository, ISuppliersGetterService suppliersGetterService, ILogger<DeliveriesAdderService> logger)
        {
            _deliveriesRepository = deliveriesRepository;
            _suppliersGetterService = suppliersGetterService;
            _logger = logger;
        }

        public async Task<DeliveryResponse> AddDelivery(DeliveryAddRequest? deliveryAddRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddDelivery), nameof(DeliveriesAdderService));

            if (deliveryAddRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryAddRequest), nameof(AddDelivery), nameof(DeliveriesAdderService));
                throw new ArgumentNullException(nameof(deliveryAddRequest));
            }

            ValidationHelper.ModelValidation(deliveryAddRequest);

            var supplier = await _suppliersGetterService.GetSupplierByID(deliveryAddRequest.SupplierID);
            if (supplier is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(supplier), nameof(AddDelivery), nameof(DeliveriesAdderService));
                throw new ArgumentException(nameof(supplier));
            }

            var delivery = deliveryAddRequest.ToDelivery();
            delivery.DeliveryID = Guid.NewGuid();
            var addedDelivery = await _deliveriesRepository.AddDelivery(delivery);

            return addedDelivery.ToDeliveryResponse();
        }
    }
}
