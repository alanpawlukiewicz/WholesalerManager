using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.ProductDTO
{
    /// <summary>
    /// DTO class for sending product data to the client.
    /// </summary>
    public class ProductResponse
    {
        public Guid ProductID { get; set; }

        public string? ProductName { get; set; }

        public string SKU { get; set; } = "";

        public string? ProductDescription { get; set; }

        public Guid? CategoryID { get; set; }

        public string? CategoryName { get; set; }

        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public string FormattedUnitPrice => UnitPrice.ToString("0.#0");


        public ProductUpdateRequest ToProductUpdateRequest()
        {
            return new ProductUpdateRequest()
            {
                ProductID = ProductID,
                ProductName = ProductName,
                SKU = SKU,
                ProductDescription = ProductDescription,
                CategoryID = CategoryID,
                UnitPrice = UnitPrice.ToString(),
                StockQuantity = StockQuantity,
                ReorderLevel = ReorderLevel
            };
        }

        public ProductDeleteRequest ToProductDeleteRequest()
        {
            return new ProductDeleteRequest()
            {
                ProductID = ProductID,
                ProductName = ProductName,
                SKU = SKU,
                StockQuantity = StockQuantity
            };
        }
    }

    public static class ProductExtensions
    {
        /// <summary>
        /// Extension method converting a Product entity to a ProductResponse DTO.
        /// </summary>
        /// <param name="product">The Product entity to convert.</param>
        /// <returns>A ProductResponse DTO.</returns>
        public static ProductResponse ToProductResponse(this Product product)
        {
            return new ProductResponse()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                SKU = product.SKU,
                ProductDescription = product.ProductDescription,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName,
                UnitPrice = product.UnitPrice,
                StockQuantity = product.StockQuantity,
                ReorderLevel = product.ReorderLevel
            };
        }
    }
}
