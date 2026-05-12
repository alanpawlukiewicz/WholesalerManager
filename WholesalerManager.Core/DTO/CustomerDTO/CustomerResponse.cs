using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.CustomerDTO
{
    public class CustomerResponse
    {
        public Guid CustomerID { get; set; }

        public string? CompanyName { get; set; }

        public string? TIN { get; set; }

        public string? ContactEmail { get; set; }

        public string? Address { get; set; }
    }

    public static class CustomerExtensions
    {
        public static CustomerResponse ToCustomerResponse(this Customer customer)
        {
            return new CustomerResponse()
            {
                CustomerID = customer.CustomerID,
                CompanyName = customer.CompanyName,
                TIN = customer.TIN,
                ContactEmail = customer.ContactEmail,
                Address = customer.Address,
            };
        }
    }
}
