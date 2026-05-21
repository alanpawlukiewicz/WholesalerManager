using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersStockCheckerService : IOrdersStockCheckerService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IOrderItemsRepository _itemsRepository;
        private readonly IProductsRepository _productsRepository;

        public OrdersStockCheckerService(IOrdersRepository ordersRepository, IOrderItemsRepository itemsRepository, IProductsRepository productsRepository)
        {
            _ordersRepository = ordersRepository;
            _itemsRepository = itemsRepository;
            _productsRepository = productsRepository;
        }

        public async Task<bool> CheckStockAvailabilityForOrder(Guid? orderID)
        {
            if (orderID is null)
            {
                throw new ArgumentNullException(nameof(orderID));
            }
            Order? matchingOrder = await _ordersRepository.GetOrderByID(orderID.Value);

            if (matchingOrder is null)
            {
                throw new ArgumentNullException(nameof(matchingOrder));
            }

            var itemsFromOrder = await _itemsRepository.GetAllOrderItemsFromOrder(orderID.Value);

            if (itemsFromOrder is null)
            {
                throw new ArgumentNullException(nameof(itemsFromOrder));
            }

            foreach (var item in itemsFromOrder)
            {
                if (item.ProductID is null)
                {
                    throw new ArgumentNullException(nameof(itemsFromOrder));
                }

                var matchingProduct = await _productsRepository.GetProductById(item.ProductID.Value);

                if (matchingProduct is null)
                {
                    throw new ArgumentNullException(nameof(matchingProduct));
                }

                if (matchingProduct.StockQuantity < item.Quantity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
