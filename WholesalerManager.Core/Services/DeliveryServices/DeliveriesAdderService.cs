using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesAdderService : IDeliveriesAdderService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;
        private readonly ISuppliersGetterService _suppliersGetterService;

        public DeliveriesAdderService(IDeliveriesRepository deliveriesRepository, ISuppliersGetterService suppliersGetterService)
        {
            _deliveriesRepository = deliveriesRepository;
            _suppliersGetterService = suppliersGetterService;
        }

        public async Task<DeliveryResponse> AddDelivery(DeliveryAddRequest? deliveryAddRequest)
        {
            if (deliveryAddRequest is null)
            {
                throw new ArgumentNullException(nameof(deliveryAddRequest));
            }

            ValidationHelper.ModelValidation(deliveryAddRequest);

            var supplier = await _suppliersGetterService.GetSupplierByID(deliveryAddRequest.SupplierID);
            if (supplier is null)
            {
                throw new ArgumentException(nameof(supplier));
            }

            var delivery = deliveryAddRequest.ToDelivery();
            delivery.DeliveryID = Guid.NewGuid();
            delivery.DeliveryDate = delivery.DeliveryDate?.AddDays(supplier.LeadTime);
            var addedDelivery = await _deliveriesRepository.AddDelivery(delivery);

            return addedDelivery.ToDeliveryResponse();
        }
    }
}
