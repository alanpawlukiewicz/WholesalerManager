using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.Core.Services.CustomerServices
{
    public class CustomersGetterService : ICustomersGetterService
    {
        private readonly ICustomersRepository _customersRepository;
        private readonly ILogger<CustomersGetterService> _logger;

        public CustomersGetterService(ICustomersRepository customersRepository, ILogger<CustomersGetterService> logger)
        {
            _customersRepository = customersRepository;
            _logger = logger;
        }

        public async Task<List<CustomerResponse>> GetAllCustomers()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllCustomers), nameof(CustomersGetterService));

            var customers = await _customersRepository.GetAllCustomers();
            return customers.Select(c => c.ToCustomerResponse()).ToList();
        }

        public async Task<CustomerResponse?> GetCustomerByID(Guid? customerID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetCustomerByID), nameof(CustomersGetterService));

            if (customerID is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(customerID), nameof(GetCustomerByTIN), nameof(CustomersGetterService));
                throw new ArgumentNullException(nameof(customerID));
            }
            var foundCustomer = await _customersRepository.GetCustomerById(customerID.Value);

            return foundCustomer?.ToCustomerResponse();
        }

        public async Task<CustomerResponse?> GetCustomerByTIN(string? TIN)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetCustomerByTIN), nameof(CustomersGetterService));

            if (TIN is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(TIN), nameof(GetCustomerByTIN), nameof(CustomersGetterService));
                throw new ArgumentNullException($"TIN {TIN}");
            }
            var matchingCustomer = await _customersRepository.GetCustomerByTIN(TIN);
            return matchingCustomer?.ToCustomerResponse();
        }

        public async Task<List<CustomerResponse>> GetFilteredCustomers(string? propertyName, string? filter, bool ignoreCase = true)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetFilteredCustomers), nameof(CustomersGetterService));

            var allCustomers = await _customersRepository.GetAllCustomers();
            var customerResponses = allCustomers.Select(c => c.ToCustomerResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(filter))
            {
                return customerResponses;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(CustomerResponse.CustomerName):
                    return customerResponses.Where(c => c.CustomerName != null && c.CustomerName.Contains(filter, stringComparisonType)).ToList();
                case nameof(CustomerResponse.TIN):
                    return customerResponses.Where(c => c.TIN != null && c.TIN.Contains(filter, stringComparisonType)).ToList();
                case nameof(CustomerResponse.ContactEmail):
                    return customerResponses.Where(c => c.ContactEmail != null && c.ContactEmail.Contains(filter, stringComparisonType)).ToList();
                case nameof(CustomerResponse.Address):
                    return customerResponses.Where(c => c.Address != null && c.Address.Contains(filter, stringComparisonType)).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }

        }

        public async Task<List<CustomerResponse>> GetSortedCustomers(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSortedCustomers), nameof(CustomersGetterService));

            var allCustomers = await _customersRepository.GetAllCustomers();
            var customerResponses = allCustomers.Select(c => c.ToCustomerResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return customerResponses;
            }

            switch (propertyName)
            {
                case nameof(CustomerResponse.CustomerName):
                    return sortOrder == SortOrderOptions.ASC
                        ? customerResponses.OrderBy(c => c.CustomerName).ToList()
                        : customerResponses.OrderByDescending(c => c.CustomerName).ToList();
                case nameof(CustomerResponse.TIN):
                    return sortOrder == SortOrderOptions.ASC
                        ? customerResponses.OrderBy(c => c.TIN).ToList()
                        : customerResponses.OrderByDescending(c => c.TIN).ToList();
                case nameof(CustomerResponse.ContactEmail):
                    return sortOrder == SortOrderOptions.ASC
                        ? customerResponses.OrderBy(c => c.ContactEmail).ToList()
                        : customerResponses.OrderByDescending(c => c.ContactEmail).ToList();
                case nameof(CustomerResponse.Address):
                    return sortOrder == SortOrderOptions.ASC
                        ? customerResponses.OrderBy(c => c.Address).ToList()
                        : customerResponses.OrderByDescending(c => c.Address).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }
    }
}
