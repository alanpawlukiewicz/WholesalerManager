using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesalerManager.Core.Domain.Entities
{
    public class InventoryLog
    {
        [Key]
        public Guid InventoryLogID { get; set; }
        public Guid ProductID { get; set; }

        [StringLength(20)]
        public string OperationType { get; set; } = string.Empty;
        public int QuantityChanged { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public Guid? OrderID { get; set; }
        public Guid? DeliveryID { get; set; }
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }

        [ForeignKey("OrderID")]
        public Order? Order { get; set; }

        [ForeignKey("DeliveryID")]
        public Delivery? Delivery { get; set; }

    }
}
