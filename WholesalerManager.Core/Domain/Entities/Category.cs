using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Category
    {
        [Key]
        public Guid CategoryID { get; set; }

        [StringLength(50)]
        public string? CategoryName { get; set; }
    }
}
