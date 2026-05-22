using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.SupplierDTO;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;

namespace WholesalerManager.Core.Services.SupplierServices
{
    public class SuppliersGetterService : ISuppliersGetterService
    {
        private readonly ISuppliersRepository _suppliersRepository;
        private readonly ILogger<SuppliersGetterService> _logger;
        public SuppliersGetterService(ISuppliersRepository suppliersRepository, ILogger<SuppliersGetterService> logger)
        {
            _suppliersRepository = suppliersRepository;
            _logger = logger;
        }

        public async Task<List<SupplierResponse>> GetAllSuppliers()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllSuppliers), nameof(SuppliersGetterService));

            var suppliers = await _suppliersRepository.GetAllSuppliers();
            return suppliers.Select(s => s.ToSupplierResponse()).ToList();
        }

        public async Task<SupplierResponse?> GetSupplierByID(Guid? supplierID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSupplierByID), nameof(SuppliersGetterService));

            if (supplierID is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(supplierID), nameof(GetSupplierByID), nameof(SuppliersGetterService));
                throw new ArgumentNullException(nameof(supplierID));
            }
            var foundSupplier = await _suppliersRepository.GetSupplierByID(supplierID.Value);
            return foundSupplier?.ToSupplierResponse();
        }
    }
}
