using WholesalerManager.Core.DTO.CustomerDTO;

namespace WholesalerManager.Core.ServiceContracts.CustomerServiceContracts
{
    /// <summary>
    /// Defines a service for updating existing customer information asynchronously.
    /// </summary>
    public interface ICustomersUpdaterService
    {
        /// <summary>
        /// Updates the details of an existing customer using the specified update request.
        /// </summary>
        /// <param name="customerUpdateRequest">An object containing the updated customer information. Cannot be null. The request must include the customer
        /// identifier and the fields to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a CustomerResponse with the
        /// updated customer details.</returns>
        Task<bool> UpdateCustomer(CustomerUpdateRequest? customerUpdateRequest);
    }
}
