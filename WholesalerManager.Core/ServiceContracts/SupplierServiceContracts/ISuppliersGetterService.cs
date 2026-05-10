using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.SupplierDTO;

namespace WholesalerManager.Core.ServiceContracts.SupplierServiceContracts
{
    /// <summary>
    /// Represents buisness logic for retrieving supplier data from the database and sending it to the client.
    /// </summary>
    public interface ISuppliersGetterService
    {
        /// <summary>
        /// Asynchronously retrieves all suppliers.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="SupplierResponse"/> DTO objects representing all suppliers. The list is empty if no suppliers are found.</returns>
        Task<List<SupplierResponse>> GetAllSuppliers();

        /// <summary>
        /// Asynchronously retrieves the supplier details for the specified supplier identifier.
        /// </summary>
        /// <param name="supplierID">The unique identifier of the supplier to retrieve. If null, the method will not return a supplier.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SupplierResponse"/> DTO with the
        /// supplier details if found; otherwise, null.</returns>
        Task<SupplierResponse?> GetSupplierByID(Guid? supplierID);
    }
}
