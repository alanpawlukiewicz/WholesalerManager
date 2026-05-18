using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Enums;

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

        /// <summary>
        /// Asynchronously retrieves customer details for specified TIN.
        /// </summary>
        /// <param name="TIN">The Tax Identification Number of the customer to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="CustomerResponse"/>
        /// with the customer's details if found; otherwise, <see langword="null"/>.</returns>
        Task<CustomerResponse?> GetCustomerByTIN(string? TIN);

        /// <summary>
        /// Asynchronously retrieves a list of customers that match the specified filter criteria based on the given property name and filter value.
        /// </summary>
        /// <param name="propertyName">Property names used for filtering</param>
        /// <param name="filter">The filter value</param>
        /// <returns>Filtered list of customers.</returns>
        Task<List<CustomerResponse>> GetFilteredCustomers(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Asynchronously retrieves a list of customers sorted by the specified property name and sort order.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by.</param>
        /// <param name="sortOrder">The order in which to sort the customers.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="CustomerResponse"/> objects representing the sorted customers.</returns>
        Task<List<CustomerResponse>> GetSortedCustomers(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}
