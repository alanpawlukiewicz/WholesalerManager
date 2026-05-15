using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
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

        public async Task<bool> DeleteCustomer(CustomerDeleteRequest? customerDeleteRequest)
        {
            if (customerDeleteRequest is null)
            {
                throw new ArgumentNullException(nameof(customerDeleteRequest));
            }

            ValidationHelper.ModelValidation(customerDeleteRequest);

            return await _customersRepository.DeleteCustomer(customerDeleteRequest.CustomerID);
        }
    }
}
