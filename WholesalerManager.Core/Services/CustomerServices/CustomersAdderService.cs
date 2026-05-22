using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CustomersAdderService> _logger;

        public CustomersAdderService(ICustomersRepository customersRepository, ICustomersGetterService customersGetterService, ILogger<CustomersAdderService> logger)
        {
            _customersRepository = customersRepository;
            _customersGetterService = customersGetterService;
            _logger = logger;
        }

        public async Task<bool> AddCustomer(CustomerAddRequest? customerAddRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddCustomer), nameof(CustomersAdderService));
            if (customerAddRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(customerAddRequest), nameof(AddCustomer), nameof(CustomersAdderService));
                throw new ArgumentNullException(nameof(customerAddRequest));
            }

            ValidationHelper.ModelValidation(customerAddRequest);

            // Check if TIN is unique
            CustomerResponse? matchingCustomer = await _customersGetterService.GetCustomerByTIN(customerAddRequest.TIN);
            if (matchingCustomer is not null)
            {
                _logger.LogWarning("User with given TIN already exists.");
                return false;
            }
            var customerToAdd = customerAddRequest.ToCustomer();
            customerToAdd.CustomerID = Guid.NewGuid();
            await _customersRepository.AddNewCustomer(customerToAdd);
            return true;
        }
    }
}
