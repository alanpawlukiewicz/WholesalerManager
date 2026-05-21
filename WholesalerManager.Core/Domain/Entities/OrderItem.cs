using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesalerManager.Core.Domain.Entities
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemID { get; set; }
        public Guid? OrderID { get; set; }
        public Guid? ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtSale { get; set; }

        [ForeignKey("OrderID")]
        public Order? Order { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }

    }
}
