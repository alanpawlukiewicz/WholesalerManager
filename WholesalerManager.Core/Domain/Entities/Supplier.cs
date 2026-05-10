using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Supplier
    {
        [Key]
        public Guid SupplierID { get; set; }

        [StringLength(50)]
        public string? SupplierName { get; set; }

        [StringLength(50)]
        public string? ContactEmail { get; set; }

        public int LeadTime { get; set; }
    }
}
