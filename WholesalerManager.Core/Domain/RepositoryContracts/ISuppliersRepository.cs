using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents data logic for managing suppliers.
    /// </summary>
    public interface ISuppliersRepository
    {
        /// <summary>
        /// Asynchronously retrieves a list of all suppliers.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Supplier"/>
        /// objects representing all suppliers. The list will be empty if no suppliers are found.</returns>
        Task<List<Supplier>> GetAllSuppliers();

        /// <summary>
        /// Asynchronously retrieves supplier by given ID.
        /// </summary>
        /// <param name="supplierID">The unique identifier of the supplier.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Supplier"/>
        /// object representing the supplier with the specified ID, or <c>null</c> if no supplier is found.</returns>
        Task<Supplier?> GetSupplierByID(Guid supplierID);
    }
}
