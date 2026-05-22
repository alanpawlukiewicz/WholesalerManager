using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class CustomersRepository : ICustomersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CustomersRepository> _logger;

        public CustomersRepository(ApplicationDbContext db, ILogger<CustomersRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Customer> AddNewCustomer(Customer customer)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddNewCustomer), nameof(CustomersRepository));

            await _db.Customer.AddAsync(customer);
            await _db.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomer(Guid customerID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteCustomer), nameof(CustomersRepository));

            var foundCustomer = await GetCustomerById(customerID);
            if (foundCustomer is null)
            {
                _logger.LogWarning("Customer not found.");
                return false;
            }
            _db.Customer.Remove(foundCustomer);
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllCustomers), nameof(CustomersRepository));

            return await _db.Customer.ToListAsync();
        }

        public async Task<Customer?> GetCustomerById(Guid customerID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetCustomerById), nameof(CustomersRepository));

            return await _db.Customer.FirstOrDefaultAsync(c => c.CustomerID == customerID);
        }

        public async Task<Customer?> GetCustomerByTIN(string TIN)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetCustomerByTIN), nameof(CustomersRepository));

            return await _db.Customer.FirstOrDefaultAsync(c => c.TIN == TIN);
        }

        public async Task<bool> UpdateCustomer(Customer customer)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateCustomer), nameof(CustomersRepository));

            Customer? matchingCustomer = await GetCustomerById(customer.CustomerID);
            if (matchingCustomer is null)
            {
                _logger.LogWarning("Customer not found.");
                return false;
            }

            matchingCustomer.CustomerName = customer.CustomerName;
            matchingCustomer.TIN = customer.TIN;
            matchingCustomer.ContactEmail = customer.ContactEmail;
            matchingCustomer.Address = customer.Address;

            await _db.SaveChangesAsync();

            return true;
        }
    }
}
