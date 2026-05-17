using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Helpers;

namespace WholesalerManager.Core.DTO.ProductDTO
{
    public class ProductUpdateRequest
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public Guid ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200)]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "SKU is required.")]
        [MaxLength(200)]
        public string SKU { get; set; } = "";

        [MaxLength(200)]
        public string? ProductDescription { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public Guid? CategoryID { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [RegularExpression("^\\d+([.,]\\d{1,2})?$", ErrorMessage = "Unit price must be of money type.")]
        public string? UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public Product ToProduct()
        {
            return new Product()
            {
                ProductID = ProductID,
                ProductName = ProductName,
                SKU = SKU,
                ProductDescription = ProductDescription,
                CategoryID = CategoryID,
                UnitPrice = UnitPrice.ToDecimalSafe(),
                StockQuantity = StockQuantity,
                ReorderLevel = ReorderLevel
            };
        }
    }
}
