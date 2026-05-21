using System.ComponentModel.DataAnnotations;

namespace WholesalerManager.Core.Domain.Entities
{
    public class Customer
    {
        [Key]
        public Guid CustomerID { get; set; }

        [StringLength(50)]
        public string? CustomerName { get; set; }

        [StringLength(9)]
        public string? TIN { get; set; }

        [StringLength(50)]
        public string? ContactEmail { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }
    }
}
