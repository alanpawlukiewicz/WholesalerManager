using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersDeleterService : ICustomersDeleterService
    {
        private readonly ICustomersRepository _customersRepository;
        private readonly ILogger<CustomersDeleterService> _logger;

        public CustomersDeleterService(ICustomersRepository customersRepository, ILogger<CustomersDeleterService> logger)
        {
            _customersRepository = customersRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteCustomer(CustomerDeleteRequest? customerDeleteRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteCustomer), nameof(CustomersDeleterService));
            if (customerDeleteRequest is null || customerDeleteRequest.CustomerID == Guid.Empty)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(customerDeleteRequest), nameof(DeleteCustomer), nameof(CustomersDeleterService));
                throw new ArgumentNullException(nameof(customerDeleteRequest));
            }

            ValidationHelper.ModelValidation(customerDeleteRequest);

            return await _customersRepository.DeleteCustomer(customerDeleteRequest.CustomerID);
        }
    }
}
