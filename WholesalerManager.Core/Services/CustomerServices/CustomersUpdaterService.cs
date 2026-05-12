using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersUpdaterService : ICustomersUpdaterService
    {
        private readonly ICustomersRepository _customersRepository;
        private readonly ICustomersGetterService _customersGetterService;

        public CustomersUpdaterService(ICustomersRepository customersRepository, ICustomersGetterService customersGetterService)
        {
            _customersRepository = customersRepository;
            _customersGetterService = customersGetterService;
        }

        public async Task<bool> UpdateCustomer(CustomerUpdateRequest? customerUpdateRequest)
        {
            if (customerUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(customerUpdateRequest));
            }
            var matchingPerson = await _customersGetterService.GetCustomerByTIN(customerUpdateRequest.TIN);

            // Check if updated TIN isn't already in table
            if (matchingPerson is not null && matchingPerson.CustomerID != customerUpdateRequest.CustomerID)
            {
                return false;
            }

            return await _customersRepository.UpdateCustomer(customerUpdateRequest.ToCustomer());
        }
    }
}
