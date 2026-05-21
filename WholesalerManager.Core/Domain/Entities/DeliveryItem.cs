using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesalerManager.Core.Domain.Entities
{
    public class DeliveryItem
    {
        [Key]
        public Guid DeliveryItemID { get; set; }
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
