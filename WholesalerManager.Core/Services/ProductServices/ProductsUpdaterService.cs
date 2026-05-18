using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsUpdaterService : IProductsUpdaterService
    {
        private readonly IProductsRepository _productsRepository;

        public ProductsUpdaterService(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<ProductResponse> UpdateProduct(ProductUpdateRequest? productUpdateRequest)
        {
            if (productUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(productUpdateRequest));
            }

            ValidationHelper.ModelValidation(productUpdateRequest);

            Product? matchingProduct = await _productsRepository.GetProductById(productUpdateRequest.ProductID);

            if (matchingProduct is null)
            {
                throw new ArgumentException(nameof(matchingProduct));
            }

            Product updatedProduct = productUpdateRequest.ToProduct();

            await _productsRepository.UpdateProduct(updatedProduct);

            return updatedProduct.ToProductResponse();
        }

        public async Task<bool> UpdateStockQuantity(EditStockQuantityDTO? editStockQuantityDTO)
        {
            if (editStockQuantityDTO is null)
            {
                throw new ArgumentNullException(nameof(editStockQuantityDTO));
            }

            if(editStockQuantityDTO.ProductID == Guid.Empty || editStockQuantityDTO.NewStockQuantity < 0)
            {
                return false;
            }

            Product? matchingProduct = await _productsRepository.GetProductById(editStockQuantityDTO.ProductID);

            if (matchingProduct is null)
            {
                return false;
            }

            matchingProduct.StockQuantity = editStockQuantityDTO.NewStockQuantity;
            await _productsRepository.Save();

            return true;

        }

        public async Task<bool> UpdateUnitPrice(EditUnitPriceDTO? editUnitPriceDTO)
        {
            if (editUnitPriceDTO is null)
            {
                throw new ArgumentNullException(nameof(editUnitPriceDTO));
            }

            decimal newUnitPriceDecimal = editUnitPriceDTO.NewUnitPrice.ToDecimalSafe();
            if (newUnitPriceDecimal <= 0 || editUnitPriceDTO.ProductID == Guid.Empty)
            {
                return false;
            }

            Product? matchingProduct = await _productsRepository.GetProductById(editUnitPriceDTO.ProductID);

            if (matchingProduct is null)
            {
                return false;
            }

            matchingProduct.UnitPrice = newUnitPriceDecimal;
            await _productsRepository.Save();

            return true;

        }
    }
}
