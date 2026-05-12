using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
