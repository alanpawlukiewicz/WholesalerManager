using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
