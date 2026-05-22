using AutoFixture;
using FluentAssertions;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;
using Xunit;

namespace WholesalerManager.ServiceTests.OrderItemServiceTests
{
    

    public class OrderItemsGetterServiceTests
    {
        private readonly Mock<IOrderItemsRepository> _orderItemsRepositoryMock;
        private readonly IFixture _fixture;
        private readonly IOrderItemsGetterService _sut;

        public OrderItemsGetterServiceTests()
        {
            _orderItemsRepositoryMock = new Mock<IOrderItemsRepository>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrderItemsGetterService(_orderItemsRepositoryMock.Object);
        }

        #region Helpers

        // Creates an OrderItem with a Product navigation property to ensure ToOrderItemResponse() maps correctly
        private OrderItem CreateOrderItem(Action<OrderItem>? configure = null)
        {
            var item = _fixture.Build<OrderItem>()
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();

            configure?.Invoke(item);
            return item;
        }

        #endregion

        #region GetAllOrderItems

        [Fact]
        public async Task GetAllOrderItems_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no items
            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItems())
                .ReturnsAsync(new List<OrderItem>());

            // Act
            var result = await _sut.GetAllOrderItems();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllOrderItems_WithItems_ReturnsAllMappedToOrderItemResponse()
        {
            // Arrange – AutoFixture generates a list of random order items
            var items = Enumerable.Range(0, 3)
                .Select(_ => CreateOrderItem())
                .ToList();

            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItems())
                .ReturnsAsync(items);

            // Act
            var result = await _sut.GetAllOrderItems();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<OrderItemResponse>();
            result.Select(r => r.OrderItemID)
                  .Should().BeEquivalentTo(items.Select(i => i.OrderItemID));
        }

        [Fact]
        public async Task GetAllOrderItems_WithItems_MapsFieldsCorrectly()
        {
            // Arrange – single item to verify field mapping precisely
            var item = CreateOrderItem();

            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItems())
                .ReturnsAsync(new List<OrderItem> { item });

            // Act
            var result = await _sut.GetAllOrderItems();

            // Assert – all fields are correctly mapped to the response DTO
            var response = result.Single();
            response.OrderItemID.Should().Be(item.OrderItemID);
            response.OrderID.Should().Be(item.OrderID);
            response.ProductID.Should().Be(item.ProductID);
            response.Quantity.Should().Be(item.Quantity);
            response.PriceAtSale.Should().Be(item.PriceAtSale);
            response.ProductName.Should().Be(item.Product?.ProductName);
            response.SKU.Should().Be(item.Product?.SKU);
            response.ProductUnitPrice.Should().Be(item.Product?.UnitPrice);
        }

        #endregion

        #region GetAllOrderItemsFromOrder

        [Fact]
        public async Task GetAllOrderItemsFromOrder_NullOrderId_ReturnsNull()
        {
            // Act
            var result = await _sut.GetAllOrderItemsFromOrder(null);

            // Assert – repository should never be called when ID is null
            result.Should().BeNull();
            _orderItemsRepositoryMock.Verify(
                r => r.GetAllOrderItemsFromOrder(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAllOrderItemsFromOrder_ValidOrderId_ReturnsEmptyListWhenNoItems()
        {
            // Arrange – order exists but has no items
            var orderId = _fixture.Create<Guid>();

            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem>());

            // Act
            var result = await _sut.GetAllOrderItemsFromOrder(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllOrderItemsFromOrder_ValidOrderId_ReturnsAllItemsMappedToOrderItemResponse()
        {
            // Arrange
            var orderId = _fixture.Create<Guid>();
            var items = Enumerable.Range(0, 3)
                .Select(_ => CreateOrderItem(i => i.OrderID = orderId))
                .ToList();

            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(items);

            // Act
            var result = await _sut.GetAllOrderItemsFromOrder(orderId);

            // Assert – all items belong to the requested order and are correctly mapped
            result.Should().NotBeNull();
            result!.Should().HaveCount(3);
            result.Should().AllBeOfType<OrderItemResponse>();
            result.Should().OnlyContain(r => r.OrderID == orderId);
        }

        [Fact]
        public async Task GetAllOrderItemsFromOrder_ValidOrderId_MapsFieldsCorrectly()
        {
            // Arrange – single item to verify field mapping precisely
            var orderId = _fixture.Create<Guid>();
            var item = CreateOrderItem(i => i.OrderID = orderId);

            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem> { item });

            // Act
            var result = await _sut.GetAllOrderItemsFromOrder(orderId);

            // Assert
            var response = result!.Single();
            response.OrderItemID.Should().Be(item.OrderItemID);
            response.OrderID.Should().Be(item.OrderID);
            response.ProductID.Should().Be(item.ProductID);
            response.Quantity.Should().Be(item.Quantity);
            response.PriceAtSale.Should().Be(item.PriceAtSale);
            response.ProductName.Should().Be(item.Product?.ProductName);
            response.SKU.Should().Be(item.Product?.SKU);
            response.ProductUnitPrice.Should().Be(item.Product?.UnitPrice);
        }

        [Fact]
        public async Task GetAllOrderItemsFromOrder_ValidOrderId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var orderId = _fixture.Create<Guid>();

            _orderItemsRepositoryMock
                .Setup(r => r.GetAllOrderItemsFromOrder(orderId))
                .ReturnsAsync(new List<OrderItem>());

            // Act
            await _sut.GetAllOrderItemsFromOrder(orderId);

            // Assert – repository should be called exactly once with the correct ID
            _orderItemsRepositoryMock.Verify(
                r => r.GetAllOrderItemsFromOrder(orderId), Times.Once);
        }

        #endregion
    }
}
