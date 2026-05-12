using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.RepositoryContracts
{
    /// <summary>
    /// Represents data layer logic for managing Customer table
    /// </summary>
    public interface ICustomersRepository
    {
        /// <summary>
        /// Asynchronously retrieves all customers from the data source.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all customers. The
        /// list is empty if no customers are found.</returns>
        Task<List<Customer>> GetAllCustomers();

        /// <summary>
        /// Asynchronously retrieves a customer by the specified unique identifier.
        /// </summary>
        /// <param name="customerID">The unique identifier of the customer to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the customer associated with the
        /// specified identifier, or null if no customer is found.</returns>
        Task<Customer?> GetCustomerById(Guid customerID);

        /// <summary>
        /// Asynchronously retrieves a customer by the specified TIN.
        /// </summary>
        /// <param name="TIN"></param>
        /// <returns></returns>
        Task<Customer?> GetCustomerByTIN(string TIN);

        /// <summary>
        /// Asynchronously adds new customer.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<Customer> AddNewCustomer(Customer customer);

        /// <summary>
        /// Asynchronously updates customer.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<bool> UpdateCustomer(Customer customer);
    }
}
