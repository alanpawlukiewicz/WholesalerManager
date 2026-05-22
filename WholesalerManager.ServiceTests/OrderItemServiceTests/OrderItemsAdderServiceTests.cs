using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;
using Xunit;

namespace WholesalerManager.ServiceTests.OrderItemServiceTests
{
    

    public class OrderItemsAdderServiceTests
    {
        private readonly Mock<IOrderItemsRepository> _orderItemsRepositoryMock;
        private readonly Mock<ILogger<OrderItemsAdderService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrderItemsAdderService _sut;

        public OrderItemsAdderServiceTests()
        {
            _orderItemsRepositoryMock = new Mock<IOrderItemsRepository>();
            _loggerMock = new Mock<ILogger<OrderItemsAdderService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrderItemsAdderService(_orderItemsRepositoryMock.Object, _loggerMock.Object);
        }

        #region Helpers

        // Creates a valid OrderItemAddRequest with all required fields filled
        private OrderItemAddRequest CreateValidItemAddRequest(Action<OrderItemAddRequest>? configure = null)
        {
            var request = _fixture.Build<OrderItemAddRequest>()
                .With(r => r.OrderID, Guid.NewGuid())
                .With(r => r.ProductID, Guid.NewGuid())
                .With(r => r.Quantity, _fixture.Create<int>())
                .With(r => r.PriceAtSale, "19.99")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates an OrderItem entity returned by the repository after adding
        private OrderItem CreateAddedOrderItem(OrderItemAddRequest request)
        {
            return _fixture.Build<OrderItem>()
                .With(i => i.OrderID, request.OrderID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();
        }

        #endregion

        #region AddOrderItem

        [Fact]
        public async Task AddOrderItem_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.AddOrderItem(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddOrderItem_MissingProductID_ThrowsArgumentException()
        {
            // Arrange – ProductID is required by validation
            var request = CreateValidItemAddRequest(r => r.ProductID = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.345")]
        [InlineData("-5.00")]
        public async Task AddOrderItem_InvalidPriceAtSale_ThrowsArgumentException(string? price)
        {
            // Arrange – PriceAtSale regex rejects non-money formats; null and empty string
            // pass validation because the field has no [Required] attribute
            var request = CreateValidItemAddRequest(r => r.PriceAtSale = price);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AddOrderItem_NullOrEmptyPriceAtSale_DoesNotThrowAndSetsPriceToZero(string? price)
        {
            // Arrange – null and empty string pass validation (no [Required] attribute)
            // and ToDecimalSafe() converts them to 0
            var request = CreateValidItemAddRequest(r => r.PriceAtSale = price);
            var addedItem = _fixture.Build<OrderItem>()
                .With(i => i.OrderID, request.OrderID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.PriceAtSale, 0m)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.Invoking(s => s.AddOrderItem(request))
                                   .Should().NotThrowAsync();

            // Assert – price defaults to 0 when null or empty is provided
            result.Subject.PriceAtSale.Should().Be(0m);
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_ReturnsOrderItemResponseWithNewGuid()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedOrderItem(request);

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.AddOrderItem(request);

            // Assert – service should assign a new non-empty OrderItemID
            result.Should().NotBeNull();
            result.OrderItemID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_MapsFieldsCorrectlyToOrderItemResponse()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedOrderItem(request);

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.AddOrderItem(request);

            // Assert – all fields from the added entity are reflected in the response
            result.OrderID.Should().Be(addedItem.OrderID);
            result.ProductID.Should().Be(addedItem.ProductID);
            result.Quantity.Should().Be(addedItem.Quantity);
            result.PriceAtSale.Should().Be(addedItem.PriceAtSale);
            result.ProductName.Should().Be(addedItem.Product?.ProductName);
            result.SKU.Should().Be(addedItem.Product?.SKU);
            result.ProductUnitPrice.Should().Be(addedItem.Product?.UnitPrice);
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedOrderItem(request);

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);

            // Act
            await _sut.AddOrderItem(request);

            // Assert
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Once);
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_PassesItemWithNewGuidToRepository()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedOrderItem(request);
            OrderItem? capturedItem = null;

            // Capture the item passed to the repository to verify its contents
            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .Callback<OrderItem>(i => capturedItem = i)
                .ReturnsAsync(addedItem);

            // Act
            await _sut.AddOrderItem(request);

            // Assert – the item passed to the repository should have a new non-empty ID
            capturedItem.Should().NotBeNull();
            capturedItem!.OrderItemID.Should().NotBeEmpty();
            capturedItem.OrderID.Should().Be(request.OrderID);
            capturedItem.ProductID.Should().Be(request.ProductID);
            capturedItem.Quantity.Should().Be(request.Quantity);
        }

        #endregion

        #region AddMultipleOrderItems

        [Fact]
        public async Task AddMultipleOrderItems_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert – null list should throw before any repository call
            await _sut.Invoking(s => s.AddMultipleOrderItems(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddMultipleOrderItems_ListWithInvalidItem_ThrowsArgumentException()
        {
            // Arrange – one item in the list fails validation
            var validItem = CreateValidItemAddRequest();
            var invalidItem = CreateValidItemAddRequest(r => r.ProductID = null);

            // Act & Assert – validation failure on any item should throw
            await _sut.Invoking(s => s.AddMultipleOrderItems(new List<OrderItemAddRequest> { validItem, invalidItem }))
                      .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddMultipleOrderItems_EmptyList_ReturnsEmptyList()
        {
            // Arrange – empty list is valid, should return empty response list
            // Act
            var result = await _sut.AddMultipleOrderItems(new List<OrderItemAddRequest>());

            // Assert
            result.Should().BeEmpty();
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddMultipleOrderItems_ValidList_ReturnsAllAddedItemResponses()
        {
            // Arrange
            var requests = new List<OrderItemAddRequest>
        {
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
        };

            // Each request gets its own added entity returned by the repository
            foreach (var request in requests)
            {
                var addedItem = CreateAddedOrderItem(request);

                _orderItemsRepositoryMock
                    .Setup(r => r.AddOrderItem(It.Is<OrderItem>(i =>
                        i.OrderID == request.OrderID &&
                        i.ProductID == request.ProductID)))
                    .ReturnsAsync(addedItem);
            }

            // Act
            var result = await _sut.AddMultipleOrderItems(requests);

            // Assert – all items processed and returned
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<OrderItemResponse>();
        }

        [Fact]
        public async Task AddMultipleOrderItems_ValidList_CallsRepositoryOncePerItem()
        {
            // Arrange
            var requests = new List<OrderItemAddRequest>
        {
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
        };

            foreach (var request in requests)
            {
                var addedItem = CreateAddedOrderItem(request);

                _orderItemsRepositoryMock
                    .Setup(r => r.AddOrderItem(It.Is<OrderItem>(i =>
                        i.OrderID == request.OrderID &&
                        i.ProductID == request.ProductID)))
                    .ReturnsAsync(addedItem);
            }

            // Act
            await _sut.AddMultipleOrderItems(requests);

            // Assert – repository called once per item in the list
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Exactly(2));
        }

        #endregion
    }
}
