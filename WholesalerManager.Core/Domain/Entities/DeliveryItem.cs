using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WholesalerManager.Core.Domain.Entities
{
    public class DeliveryItem
    {
        public Guid? DeliveryID { get; set; }
        public Guid? ProductID { get; set; }

        public int Quantity { get; set; }

        public decimal PriceAtSale { get; set; }

        [ForeignKey("DeliveryID")]
        public Delivery? Delivery { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }
    }
}
