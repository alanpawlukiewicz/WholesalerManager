using System.ComponentModel.DataAnnotations;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.CustomerDTO
{
    public class CustomerAddRequest
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "Please enter customer's full name or company name.")]
        public string? CustomerName { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "TIN must be exactly 9 characters long.")]
        public string? TIN { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "Please enter customer's contact email.")]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Please enter customer's address.")]
        public string? Address { get; set; }

        public CustomerResponse ToCustomerResponse()
        {
            return new CustomerResponse()
            {
                CustomerName = CustomerName,
                TIN = TIN,
                ContactEmail = ContactEmail,
                Address = Address
            };
        }

        public Customer ToCustomer()
        {
            return new Customer()
            {
                CustomerName = CustomerName,
                TIN = TIN,
                ContactEmail = ContactEmail,
                Address = Address
            };
        }
    }
}
