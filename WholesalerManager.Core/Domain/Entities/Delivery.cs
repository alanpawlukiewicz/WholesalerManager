using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Delivery
    {
        [Key]
        public Guid DeliveryID { get; set; }

        public Guid? SupplierID { get; set; }

        public DateTime? OrderDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [ForeignKey("SupplierID")]
        public Supplier? Supplier { get; set; }
    }

}
