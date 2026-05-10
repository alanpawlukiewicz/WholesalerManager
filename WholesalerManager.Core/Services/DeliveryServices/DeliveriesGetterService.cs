using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesGetterService : IDeliveriesGetterService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;

        public DeliveriesGetterService(IDeliveriesRepository deliveriesRepository)
        {
            _deliveriesRepository = deliveriesRepository;
        }

        public async Task<List<DeliveryResponse>> GetAllDeliveries()
        {
            var deliveries = await _deliveriesRepository.GetAllDeliveries();
            return deliveries.Select(d => d.ToDeliveryResponse()).ToList();
        }

        public async Task<DeliveryResponse?> GetDeliveryById(Guid? deliveryID)
        {
            if (deliveryID is null)
            {
                return null;
            }
            var delivery = await _deliveriesRepository.GetDeliveryById(deliveryID.Value);
            return delivery?.ToDeliveryResponse();
        }
    }
}
