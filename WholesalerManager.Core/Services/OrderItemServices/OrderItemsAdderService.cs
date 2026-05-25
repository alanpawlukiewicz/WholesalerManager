using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.Exceptions;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.OrderItemServices
{
    public class OrderItemsAdderService : IOrderItemsAdderService
    {
        private readonly IOrderItemsRepository _orderItemsRepository;
        private readonly ILogger<OrderItemsAdderService> _logger;
        private readonly IProductsGetterService _productsGetterService;

        public OrderItemsAdderService(IOrderItemsRepository orderItemsRepository, ILogger<OrderItemsAdderService> logger, IProductsGetterService productsGetterService)
        {
            _orderItemsRepository = orderItemsRepository;
            _logger = logger;
            _productsGetterService = productsGetterService;
        }

        public async Task<OrderItemResponse> AddOrderItem(OrderItemAddRequest? itemAddRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddOrderItem), nameof(OrderItemsAdderService));
            if (itemAddRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(itemAddRequest), nameof(AddMultipleOrderItems), nameof(OrderItemsAdderService));
                throw new ArgumentNullException(nameof(itemAddRequest));
            }

            ValidationHelper.ModelValidation(itemAddRequest);

            var matchingProduct = await _productsGetterService.GetProductById(itemAddRequest.ProductID);

            if (matchingProduct is null)
            {
                _logger.LogError("Matching product does not exist.");
                throw new ArgumentNullException(nameof(matchingProduct));
            }

            if (matchingProduct.StockQuantity < itemAddRequest.Quantity)
            {
                _logger.LogError($"Item: {matchingProduct.ProductName} exceeds product's stock quantity: {matchingProduct.StockQuantity.ToString()}");
                throw new InsufficientProductStockException($"Item: {matchingProduct.ProductName} exceeds product's stock quantity: {matchingProduct.StockQuantity.ToString()}.");
            }

            var item = itemAddRequest.ToOrderItem();
            item.OrderItemID = Guid.NewGuid();
            var addedItem = await _orderItemsRepository.AddOrderItem(item);

            return addedItem.ToOrderItemResponse();

        }

        public async Task<List<OrderItemResponse>> AddMultipleOrderItems(List<OrderItemAddRequest>? itemAddRequests)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddMultipleOrderItems), nameof(OrderItemsAdderService));
            if (itemAddRequests is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(itemAddRequests), nameof(AddMultipleOrderItems), nameof(OrderItemsAdderService));
                throw new ArgumentNullException(nameof(itemAddRequests));
            }

            foreach (var item in itemAddRequests)
            {
                ValidationHelper.ModelValidation(item);
            }

            List<OrderItemResponse> addedItems = new List<OrderItemResponse>() { };

            foreach (var item in itemAddRequests)
            {
                addedItems.Add(await AddOrderItem(item));
            }

            return addedItems;
        }
    }
}
