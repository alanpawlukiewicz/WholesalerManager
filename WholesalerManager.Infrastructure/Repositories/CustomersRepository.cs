using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesaleManager.Infrastructure.DatabaseContext;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.RepositoryContracts;

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
    }
}
