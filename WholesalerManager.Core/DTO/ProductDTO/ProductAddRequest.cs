using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.ProductDTO
{
    public class ProductAddRequest
    {
        [Required(ErrorMessage = "Product name is required.")]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "SKU is required.")]
        public string SKU { get; set; } = "";

        public string? ProductDescription { get; set; }

        public Guid? CategoryID { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public Product ToProduct()
        {
            return new Product()
            {
                ProductName = ProductName,
                SKU = SKU,
                ProductDescription = ProductDescription,
                CategoryID = CategoryID,
                UnitPrice = UnitPrice,
                StockQuantity = StockQuantity,
                ReorderLevel = ReorderLevel
            };
        }
    }
}
