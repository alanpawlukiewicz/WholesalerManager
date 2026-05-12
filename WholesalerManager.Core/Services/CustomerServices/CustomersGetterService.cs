using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersGetterService : ICustomersGetterService
    {
        private readonly ICustomersRepository _customersRepository;

        public CustomersGetterService(ICustomersRepository customersRepository)
        {
            _customersRepository = customersRepository;
        }

        public async Task<List<CustomerResponse>> GetAllCustomers()
        {
            var customers = await _customersRepository.GetAllCustomers();
            return customers.Select(c => c.ToCustomerResponse()).ToList();
        }

        public async Task<CustomerResponse?> GetCustomerByID(Guid? customerID)
        {
            if (customerID is null)
            {
                throw new ArgumentNullException(nameof(customerID));
            }
            var foundCustomer = await _customersRepository.GetCustomerById(customerID.Value);

            return foundCustomer is null ? null : foundCustomer.ToCustomerResponse();
        }
    }
}
