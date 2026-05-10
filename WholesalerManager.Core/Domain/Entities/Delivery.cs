using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.ProductDTO;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Delivery
    {
        [Key]
        public Guid DeliveryID { get; set; }

        public Guid? SupplierID { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [ForeignKey("SupplierID")]
        public Supplier? Supplier { get; set; }
    }

}
