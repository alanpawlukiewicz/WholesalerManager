using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Text;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Product
    {
        [Key]
        public Guid ProductID { get; set; }

        [StringLength(50)]
        public string? ProductName { get; set; }


        [StringLength(50)]
        public string SKU { get; set; } = "";

        [StringLength(200)]
        public string? ProductDescription { get; set; }

        //[ForeignKey("Category")]
        public Guid? CategoryID { get; set; }

        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }
    }
}
