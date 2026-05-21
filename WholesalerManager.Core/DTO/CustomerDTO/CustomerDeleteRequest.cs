using System.ComponentModel.DataAnnotations;

namespace WholesalerManager.Core.DTO.CustomerDTO
{
    public class CustomerDeleteRequest
    {
        [Required()]
        public Guid CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public string? TIN { get; set; }
    }
}
