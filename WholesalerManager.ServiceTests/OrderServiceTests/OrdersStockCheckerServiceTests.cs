using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderServices;

namespace WholesalerManager.ServiceTests.OrderServiceTests
{


    public class OrdersStockCheckerServiceTests
    {
        private readonly Mock<IOrdersRepository> _ordersRepositoryMock;
        private readonly Mock<IOrderItemsRepository> _itemsRepositoryMock;
        private readonly Mock<IProductsRepository> _productsRepositoryMock;
        private readonly Mock<ILogger<OrdersStockCheckerService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrdersStockCheckerService _sut;

        public OrdersStockCheckerServiceTests()
        {
            _ordersRepositoryMock = new Mock<IOrdersRepository>();
            _itemsRepositoryMock = new Mock<IOrderItemsRepository>();
            _productsRepositoryMock = new Mock<IProductsRepository>();
            _loggerMock = new Mock<ILogger<OrdersStockCheckerService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrdersStockCheckerService(
                _ordersRepositoryMock.Object,
                _itemsRepositoryMock.Object,
                _productsRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates an OrderItem with a known ProductID and Quantity
        private OrderItem CreateOrderItem(Guid orderId, Guid productId, int quantity)
        {
            return _fixture.Build<OrderItem>()
                .With(i => i.OrderID, orderId)
                .With(i => i.ProductID, productId)
                .With(i => i.Quantity, quantity)
                .Create();
        }

        // Creates a Product with a known ProductID and StockQuantity
        private Product CreateProduct(Guid productId, int stockQuantity)
        {
            return _fixture.Build<Product>()
                .With(p => p.ProductID, productId)
                .With(p => p.StockQuantity, stockQuantity)
                .With(p => p.Category, _fixture.Create<Category>())
                .Create();
        }

        #endregion

        #region CheckStockAvailabilityForOrder

        [Fact]
        public async Task CheckStockAvailabilityForOrder_NullOrderId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.CheckStockAvailabilityForOrder(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _ordersRepositoryMock.Verify(r => r.GetOrderByID(It.IsAny<Guid>()), Times.Never);
            _itemsRepositoryMock.Verify(r => r.GetAllOrderItemsFromOrder(It.IsAny<Guid>()), Times.Never);
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_NonExistentOrder_ThrowsArgumentNullException()
        {
            // Arrange – order repository returns null for a given ID
            var orderId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync((Order?)null);

            // Act & Assert
            await _sut.Invoking(s => s.CheckStockAvailabilityForOrder(orderId))
                      .Should().ThrowAsync<ArgumentNullException>();

            _itemsRepositoryMock.Verify(r => r.GetAllOrderItemsFromOrder(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_NullItemsList_ThrowsArgumentNullException()
        {
            // Arrange – items repository returns null instead of a list
            var orderId = _fixture.Create<Guid>();
            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync((List<OrderItem>?)null);

            // Act & Assert
            await _sut.Invoking(s => s.CheckStockAvailabilityForOrder(orderId))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_ItemWithNullProductID_ThrowsArgumentNullException()
        {
            // Arrange – one of the order items has no ProductID assigned
            var orderId = _fixture.Create<Guid>();
            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            var itemWithNullProduct = _fixture.Build<OrderItem>()
                .With(i => i.OrderID, orderId)
                .Without(i => i.ProductID)   // ProductID = null
                .Create();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { itemWithNullProduct });

            // Act & Assert
            await _sut.Invoking(s => s.CheckStockAvailabilityForOrder(orderId))
                      .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_ProductNotFound_ThrowsArgumentNullException()
        {
            // Arrange – product referenced by an order item does not exist in the repository
            var orderId = _fixture.Create<Guid>();
            var productId = _fixture.Create<Guid>();

            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            var item = CreateOrderItem(orderId, productId, quantity: 5);

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { item });

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await _sut.Invoking(s => s.CheckStockAvailabilityForOrder(orderId))
                      .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_InsufficientStockForOneItem_ReturnsFalse()
        {
            // Arrange – product has less stock than the ordered quantity
            var orderId = _fixture.Create<Guid>();
            var productId = _fixture.Create<Guid>();

            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            // Order quantity (10) exceeds available stock (5)
            var item = CreateOrderItem(orderId, productId, quantity: 10);
            var product = CreateProduct(productId, stockQuantity: 5);

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { item });

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.CheckStockAvailabilityForOrder(orderId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_ExactStockMatch_ReturnsTrue()
        {
            // Arrange – stock quantity exactly equals the ordered quantity
            var orderId = _fixture.Create<Guid>();
            var productId = _fixture.Create<Guid>();

            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            var item = CreateOrderItem(orderId, productId, quantity: 10);
            var product = CreateProduct(productId, stockQuantity: 10);

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { item });

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.CheckStockAvailabilityForOrder(orderId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_SufficientStockForAllItems_ReturnsTrue()
        {
            // Arrange – all products have enough stock to fulfill their order items
            var orderId = _fixture.Create<Guid>();

            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            var productId1 = _fixture.Create<Guid>();
            var productId2 = _fixture.Create<Guid>();

            var item1 = CreateOrderItem(orderId, productId1, quantity: 3);
            var item2 = CreateOrderItem(orderId, productId2, quantity: 7);

            var product1 = CreateProduct(productId1, stockQuantity: 10);
            var product2 = CreateProduct(productId2, stockQuantity: 20);

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { item1, item2 });

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId1))
                .ReturnsAsync(product1);

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId2))
                .ReturnsAsync(product2);

            // Act
            var result = await _sut.CheckStockAvailabilityForOrder(orderId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_OneOfManyItemsHasInsufficientStock_ReturnsFalse()
        {
            // Arrange – first item has enough stock, second does not
            var orderId = _fixture.Create<Guid>();

            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            var productId1 = _fixture.Create<Guid>();
            var productId2 = _fixture.Create<Guid>();

            var item1 = CreateOrderItem(orderId, productId1, quantity: 3);
            var item2 = CreateOrderItem(orderId, productId2, quantity: 50);  // exceeds stock

            var product1 = CreateProduct(productId1, stockQuantity: 10);
            var product2 = CreateProduct(productId2, stockQuantity: 5);     // insufficient

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { item1, item2 });

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId1))
                .ReturnsAsync(product1);

            _productsRepositoryMock
                .Setup(r => r.GetProductById(productId2))
                .ReturnsAsync(product2);

            // Act
            var result = await _sut.CheckStockAvailabilityForOrder(orderId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckStockAvailabilityForOrder_EmptyItemsList_ReturnsTrue()
        {
            // Arrange – order exists but has no items, so stock is trivially sufficient
            var orderId = _fixture.Create<Guid>();

            var order = _fixture.Build<Order>()
                .With(o => o.OrderID, orderId)
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _itemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem>());

            // Act
            var result = await _sut.CheckStockAvailabilityForOrder(orderId);

            // Assert
            result.Should().BeTrue();
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        #endregion
    }
}
