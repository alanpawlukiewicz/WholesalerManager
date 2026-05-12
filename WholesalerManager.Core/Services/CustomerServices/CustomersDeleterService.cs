using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersDeleterService : ICustomersDeleterService
    {
        private readonly ICustomersRepository _customersRepository;

        public CustomersDeleterService(ICustomersRepository customersRepository)
        {
            _customersRepository = customersRepository;
        }

        public async Task<bool> DeleteCustomer(Guid? customerID)
        {
            if (customerID is null)
            {
                throw new ArgumentNullException(nameof(customerID));
            }
            return await _customersRepository.DeleteCustomer(customerID.Value);
        }
    }
}
