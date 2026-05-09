using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.ProductDTO;

namespace WholesalerManager.Core.ServiceContracts.ProductServiceContracts
{
    public interface IProductsGetterService
    {
        /// <summary>
        /// Returns all products.
        /// </summary>
        /// <returns></returns>
        Task<List<ProductResponse>> GetAllProducts();

        /// <summary>
        /// Returns product with given id.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
         Task<ProductResponse?> GetProductById(Guid? productID);
    }
}
