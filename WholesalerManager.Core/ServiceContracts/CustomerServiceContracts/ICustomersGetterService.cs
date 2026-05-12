using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.CustomerDTO;

namespace WholesalerManager.Core.ServiceContracts.CustomerServiceContracts
{
    /// <summary>
    /// Defines methods for asynchronously retrieving customer information.
    /// </summary>
    public interface ICustomersGetterService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all customers.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="CustomerResponse"/> objects representing all customers. The list will be empty if no customers are
        /// found.</returns>
        Task<List<CustomerResponse>> GetAllCustomers();

        /// <summary>
        /// Asynchronously retrieves customer details for the specified customer identifier.
        /// </summary>
        /// <param name="customerID">The unique identifier of the customer to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="CustomerResponse"/>
        /// with the customer's details if found; otherwise, <see langword="null"/>.</returns>
        Task<CustomerResponse?> GetCustomerByID(Guid? customerID);
    }
}
