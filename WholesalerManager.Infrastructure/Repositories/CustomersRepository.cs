using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class CustomersRepository : ICustomersRepository
    {
        private readonly ApplicationDbContext _db;

        public CustomersRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Customer> AddNewCustomer(Customer customer)
        {
            await _db.Customer.AddAsync(customer);
            await _db.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomer(Guid customerID)
        {
            var foundCustomer = await GetCustomerById(customerID);
            if (foundCustomer is null)
            {
                return false;
            }
            _db.Customer.Remove(foundCustomer);
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await _db.Customer.ToListAsync();
        }

        public async Task<Customer?> GetCustomerById(Guid customerID)
        {
            return await _db.Customer.FirstOrDefaultAsync(c => c.CustomerID == customerID);
        }

        public async Task<Customer?> GetCustomerByTIN(string TIN)
        {
            return await _db.Customer.FirstOrDefaultAsync(c => c.TIN == TIN);
        }

        public async Task<bool> UpdateCustomer(Customer customer)
        {
            Customer? matchingCustomer = await GetCustomerById(customer.CustomerID);
            if (matchingCustomer is null)
            {
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
