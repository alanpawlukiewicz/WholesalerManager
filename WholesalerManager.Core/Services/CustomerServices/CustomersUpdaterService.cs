using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersUpdaterService : ICustomersUpdaterService
    {
        private readonly ICustomersRepository _customersRepository;
        private readonly ICustomersGetterService _customersGetterService;
        private readonly ILogger<CustomersUpdaterService> _logger;

        public CustomersUpdaterService(ICustomersRepository customersRepository, ICustomersGetterService customersGetterService, ILogger<CustomersUpdaterService> logger)
        {
            _customersRepository = customersRepository;
            _customersGetterService = customersGetterService;
            _logger = logger;
        }

        public async Task<bool> UpdateCustomer(CustomerUpdateRequest? customerUpdateRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateCustomer), nameof(CustomersUpdaterService));
            if (customerUpdateRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(customerUpdateRequest), nameof(UpdateCustomer), nameof(CustomersUpdaterService));
                throw new ArgumentNullException(nameof(customerUpdateRequest));
            }

            ValidationHelper.ModelValidation(customerUpdateRequest);

            var matchingPerson = await _customersGetterService.GetCustomerByTIN(customerUpdateRequest.TIN);

            // Check if updated TIN isn't already in table in another record
            if (matchingPerson is not null && matchingPerson.CustomerID != customerUpdateRequest.CustomerID)
            {
                _logger.LogWarning("TIN already exists inside table.");
                return false;
            }

            return await _customersRepository.UpdateCustomer(customerUpdateRequest.ToCustomer());
        }
    }
}
