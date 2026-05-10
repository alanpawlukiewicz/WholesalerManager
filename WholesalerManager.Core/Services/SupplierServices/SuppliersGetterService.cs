using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.SupplierDTO;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;

namespace WholesalerManager.Core.Services.SupplierServices
{
    public class SuppliersGetterService : ISuppliersGetterService
    {
        private readonly ISuppliersRepository _suppliersRepository;

        public SuppliersGetterService(ISuppliersRepository suppliersRepository)
        {
            _suppliersRepository = suppliersRepository;
        }

        public async Task<List<SupplierResponse>> GetAllSuppliersAsync()
        {
            var suppliers = await _suppliersRepository.GetAllSuppliers();
            return suppliers.Select(s => s.ToSupplierResponse()).ToList();
        }

        public async Task<SupplierResponse?> GetSupplierByID(Guid? supplierID)
        {
            if (supplierID is null)
            {
                return null;
            }
            var foundSupplier = await _suppliersRepository.GetSupplierByID(supplierID.Value);
            return foundSupplier?.ToSupplierResponse();
        }
    }
}
