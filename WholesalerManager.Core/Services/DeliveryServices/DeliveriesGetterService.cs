using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class DeliveriesGetterService : IDeliveriesGetterService
    {
        private readonly IDeliveriesRepository _deliveriesRepository;
        private readonly ILogger<DeliveriesGetterService> _logger;

        public DeliveriesGetterService(IDeliveriesRepository deliveriesRepository, ILogger<DeliveriesGetterService> logger)
        {
            _deliveriesRepository = deliveriesRepository;
            _logger = logger;
        }

        public async Task<List<DeliveryResponse>> GetAllDeliveries()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllDeliveries), nameof(DeliveriesGetterService));

            var deliveries = await _deliveriesRepository.GetAllDeliveries();
            return deliveries.Select(d => d.ToDeliveryResponse()).ToList();
        }

        public async Task<DeliveryResponse?> GetDeliveryById(Guid? deliveryID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetDeliveryById), nameof(DeliveriesGetterService));

            if (deliveryID is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(deliveryID), nameof(GetDeliveryById), nameof(DeliveriesGetterService));
                throw new ArgumentNullException(nameof(deliveryID));
            }
            var delivery = await _deliveriesRepository.GetDeliveryById(deliveryID.Value);
            return delivery?.ToDeliveryResponse();
        }

        public async Task<List<DeliveryResponse>> GetFilteredDeliveries(string? propertyName, string? filter, bool ignoreCase = true)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetFilteredDeliveries), nameof(DeliveriesGetterService));

            var allDeliveries = await _deliveriesRepository.GetAllDeliveries();
            var deliveryResponses = allDeliveries.Select(d => d.ToDeliveryResponse()).ToList();

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(filter))
            {
                return deliveryResponses;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(DeliveryResponse.SupplierName):
                    return deliveryResponses.Where(d => d.SupplierName != null && d.SupplierName.Contains(filter, stringComparisonType)).ToList();
                case nameof(DeliveryResponse.Status):
                    return deliveryResponses.Where(d => d.Status != null && d.Status.Contains(filter, stringComparisonType)).ToList();
                case nameof(DeliveryResponse.OrderDate):
                    if (DateTime.TryParse(filter, out var filterDate))
                    {
                        var startDate = filterDate.Date;
                        var endDate = startDate.AddDays(1);

                        return deliveryResponses
                            .Where(d => d.OrderDate >= startDate && d.OrderDate < endDate)
                            .ToList();
                    }
                    return new List<DeliveryResponse>();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }

        public async Task<List<DeliveryResponse>> GetSortedDeliveries(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSortedDeliveries), nameof(DeliveriesGetterService));

            var allDeliveries = await _deliveriesRepository.GetAllDeliveries();
            var deliveryResponses = allDeliveries.Select(d => d.ToDeliveryResponse()).ToList();

            if (string.IsNullOrEmpty(propertyName))
            {
                return deliveryResponses;
            }

            switch (propertyName)
            {
                case nameof(DeliveryResponse.SupplierName):
                    return sortOrder == SortOrderOptions.ASC
                        ? deliveryResponses.OrderBy(d => d.SupplierName).ToList()
                        : deliveryResponses.OrderByDescending(d => d.SupplierName).ToList();
                case nameof(DeliveryResponse.Status):
                    return sortOrder == SortOrderOptions.ASC
                        ? deliveryResponses.OrderBy(d => d.Status).ToList()
                        : deliveryResponses.OrderByDescending(d => d.Status).ToList();
                case nameof(DeliveryResponse.OrderDate):
                    return sortOrder == SortOrderOptions.ASC
                        ? deliveryResponses.OrderBy(d => d.OrderDate).ToList()
                        : deliveryResponses.OrderByDescending(d => d.OrderDate).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }
    }
}
