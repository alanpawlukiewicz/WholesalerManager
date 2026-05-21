using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.SupplierDTO
{
    public class SupplierResponse
    {
        public Guid SupplierID { get; set; }

        public string? SupplierName { get; set; }

        public string? ContactEmail { get; set; }

        public int LeadTime { get; set; }
    }

    public static class SupplierExtensions
    {
        /// <summary>
        /// Extension method converting a Supplier entity to a SupplierResponse DTO.
        /// </summary>
        /// <param name="supplier">The Supplier entity to convert.</param>
        /// <returns>A SupplierResponse DTO.</returns>
        public static SupplierResponse ToSupplierResponse(this Supplier supplier)
        {
            return new SupplierResponse()
            {
                SupplierID = supplier.SupplierID,
                SupplierName = supplier.SupplierName,
                ContactEmail = supplier.ContactEmail,
                LeadTime = supplier.LeadTime
            };
        }
    }
}
