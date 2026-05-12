using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesDeleterService : IDeliveriesDeleterService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;

        public DeliveriesDeleterService(IDeliveriesRepository deliveriesRepository)
        {
            _deliveriesRepository = deliveriesRepository;
        }

        public async Task<bool> DeleteDeliveryByID(Guid? deliveryID)
        {
            if (deliveryID is null)
            {
                throw new ArgumentNullException(nameof(deliveryID));
            }

            if (deliveryID == Guid.Empty)
            {
                throw new ArgumentException(nameof(deliveryID));
            }

            return await _deliveriesRepository.DeleteDeliveryById(deliveryID.Value);
        }
    }
}
