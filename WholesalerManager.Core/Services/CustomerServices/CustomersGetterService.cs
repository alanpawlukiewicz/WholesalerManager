using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Enums;
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

            return foundCustomer?.ToCustomerResponse();
        }

        public async Task<CustomerResponse?> GetCustomerByTIN(string? TIN)
        {
            if (TIN is null)
            {
                return null;
            }
            var matchingCustomer = await _customersRepository.GetCustomerByTIN(TIN);
            return matchingCustomer?.ToCustomerResponse();
        }

        public async Task<List<CustomerResponse>> GetFilteredCustomers(string? propertyName, string? filter, bool ignoreCase = true)
        {
            var allCustomers = await _customersRepository.GetAllCustomers();
            var customerResponses =  allCustomers.Select(c => c.ToCustomerResponse()).ToList();

            if(propertyName is null || filter is null)
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
            var allCustomers = await _customersRepository.GetAllCustomers();
            var customerResponses = allCustomers.Select(c => c.ToCustomerResponse()).ToList();

            if (propertyName is null)
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
