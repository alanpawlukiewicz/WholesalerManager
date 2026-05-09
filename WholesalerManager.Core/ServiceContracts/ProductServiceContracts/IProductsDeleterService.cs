using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.ServiceContracts.ProductServiceContracts
{
    public interface IProductsDeleterService
    {
        /// <summary>
        /// Deletes product with the given productID from the database. Returns true if the product is successfully deleted, otherwise false.
        /// </summary>
        /// <param name="productID">The unique identifier of the product to be deleted.</param>
        /// <returns>True if the product was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteProduct(Guid? productID);
    }
}
