using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.CustomerDTO;

namespace WholesalerManager.Core.ServiceContracts.CustomerServiceContracts
{
    /// <summary>
    /// Defines a service for asynchronously adding a new customer to the system.
    /// </summary>
    public interface ICustomerAdderService
    {
        /// <summary>
        /// Asynchronously adds customer.
        /// </summary>
        /// <param name="customerAddRequest"></param>
        /// <returns>True if customer was succesfully added, false if otherwise.</returns>
        Task<bool> AddCustomer(CustomerAddRequest? customerAddRequest);
    }
}
