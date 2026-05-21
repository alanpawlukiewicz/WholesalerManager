using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.SupplierDTO;
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

        public async Task<List<SupplierResponse>> GetAllSuppliers()
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
