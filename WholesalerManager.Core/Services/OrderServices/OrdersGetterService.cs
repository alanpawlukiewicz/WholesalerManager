using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersGetterService : IOrdersGetterService
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrdersGetterService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        public async Task<List<OrderResponse>> GetAllOrders()
        {
            var orders = await _ordersRepository.GetAllOrders();
            return orders.Select(o => o.ToOrderResponse()).ToList();
        }

        public async Task<List<OrderResponse>> GetFilteredOrders(string? propertyName, string? filter, bool ignoreCase = true)
        {
            var allOrders = await _ordersRepository.GetAllOrders();
            var orderResponses = allOrders.Select(o => o.ToOrderResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(filter))
            {
                return orderResponses;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(OrderResponse.CustomerName):
                    return orderResponses.Where(o => o.CustomerName != null && o.CustomerName.Contains(filter, stringComparisonType)).ToList();
                case nameof(OrderResponse.TIN):
                    return orderResponses.Where(o => o.TIN != null && o.TIN.Contains(filter, stringComparisonType)).ToList();
                case nameof(OrderResponse.Status):
                    return orderResponses.Where(o => o.Status != null && o.Status.Contains(filter, stringComparisonType)).ToList();
                case nameof(OrderResponse.OrderDate):
                    if (DateTime.TryParse(filter, out var filterDate))
                    {
                        var startDate = filterDate.Date;
                        var endDate = startDate.AddDays(1);

                        return orderResponses
                            .Where(d => d.OrderDate >= startDate && d.OrderDate < endDate)
                            .ToList();
                    }
                    return new List<OrderResponse>();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }

        public async Task<OrderResponse?> GetOrderByID(Guid? orderID)
        {
            if (orderID is null)
            {
                return null;
            }
            var order = await _ordersRepository.GetOrderByID(orderID.Value);
            return order?.ToOrderResponse();
        }

        public async Task<List<OrderResponse>> GetSortedOrders(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            var allOrders = await _ordersRepository.GetAllOrders();
            var orderResponses = allOrders.Select(o => o.ToOrderResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return orderResponses;
            }

            switch (propertyName)
            {
                case nameof(OrderResponse.CustomerName):
                    return sortOrder == SortOrderOptions.ASC
                        ? orderResponses.OrderBy(o => o.CustomerName).ToList()
                        : orderResponses.OrderByDescending(o => o.CustomerName).ToList();
                case nameof(OrderResponse.TIN):
                    return sortOrder == SortOrderOptions.ASC
                        ? orderResponses.OrderBy(o => o.TIN).ToList()
                        : orderResponses.OrderByDescending(o => o.TIN).ToList();
                case nameof(OrderResponse.Status):
                    return sortOrder == SortOrderOptions.ASC
                        ? orderResponses.OrderBy(o => o.Status).ToList()
                        : orderResponses.OrderByDescending(o => o.Status).ToList();
                case nameof(OrderResponse.OrderDate):
                    return sortOrder == SortOrderOptions.ASC
                        ? orderResponses.OrderBy(o => o.OrderDate).ToList()
                        : orderResponses.OrderByDescending(o => o.OrderDate).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");

            }
        }
    }
}
