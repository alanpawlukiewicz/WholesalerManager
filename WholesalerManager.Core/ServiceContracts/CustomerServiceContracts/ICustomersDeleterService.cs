using WholesalerManager.Core.DTO.CustomerDTO;

namespace WholesalerManager.Core.ServiceContracts.CustomerServiceContracts
{
    /// <summary>
    /// Defines a service for deleting customer records by their unique identifier.
    /// </summary>
    public interface ICustomersDeleterService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        Task<bool> DeleteCustomer(CustomerDeleteRequest? customerDeleteRequest);
    }
}
