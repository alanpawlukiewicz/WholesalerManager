using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderID { get; set; }
        public Guid? CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [ForeignKey("CustomerID")]
        public Customer? Customer { get; set; }
    }
}
