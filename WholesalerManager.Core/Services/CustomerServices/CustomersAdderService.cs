using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersAdderService : ICustomersAdderService
    {
        private readonly ICustomersRepository _customersRepository;
        private readonly ICustomersGetterService _customersGetterService;

        public CustomersAdderService(ICustomersRepository customersRepository, ICustomersGetterService customersGetterService)
        {
            _customersRepository = customersRepository;
            _customersGetterService = customersGetterService;
        }

        public async Task<bool> AddCustomer(CustomerAddRequest? customerAddRequest)
        {
            if (customerAddRequest is null)
            {
                throw new ArgumentNullException(nameof(customerAddRequest));
            }

            ValidationHelper.ModelValidation(customerAddRequest);

            CustomerResponse? matchingCustomer = await _customersGetterService.GetCustomerByTIN(customerAddRequest.TIN);
            if (matchingCustomer is not null)
            {
                return false;
            }
            var customerToAdd = customerAddRequest.ToCustomer();
            customerToAdd.CustomerID = Guid.NewGuid();
            await _customersRepository.AddNewCustomer(customerToAdd);
            return true;
        }
    }
}
