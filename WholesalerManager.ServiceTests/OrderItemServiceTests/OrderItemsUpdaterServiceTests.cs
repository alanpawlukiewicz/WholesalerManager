using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;
namespace WholesalerManager.ServiceTests.OrderItemServiceTests
{


    public class OrderItemsUpdaterServiceTests
    {
        private readonly Mock<IOrderItemsRepository> _orderItemsRepositoryMock;
        private readonly Mock<IOrderItemsAdderService> _orderItemsAdderServiceMock;
        private readonly Mock<ILogger<OrderItemsUpdaterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrderItemsUpdaterService _sut;

        public OrderItemsUpdaterServiceTests()
        {
            _orderItemsRepositoryMock = new Mock<IOrderItemsRepository>();
            _orderItemsAdderServiceMock = new Mock<IOrderItemsAdderService>();
            _loggerMock = new Mock<ILogger<OrderItemsUpdaterService>>();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrderItemsUpdaterService(
                _orderItemsRepositoryMock.Object,
                _orderItemsAdderServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid OrderItemUpdateRequest with a known non-empty OrderItemID (update path)
        private OrderItemUpdateRequest CreateValidUpdateRequest(Action<OrderItemUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<OrderItemUpdateRequest>()
                .With(r => r.OrderItemID, Guid.NewGuid())
                .With(r => r.OrderID, Guid.NewGuid())
                .With(r => r.ProductID, Guid.NewGuid())
                .With(r => r.Quantity, _fixture.Create<int>())
                .With(r => r.PriceAtSale, "19.99")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates an OrderItem entity returned by the repository after updating
        private OrderItem CreateUpdatedOrderItem(OrderItemUpdateRequest request)
        {
            return _fixture.Build<OrderItem>()
                .With(i => i.OrderItemID, request.OrderItemID ?? Guid.NewGuid())
                .With(i => i.OrderID, request.OrderID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();
        }

        // Creates an OrderItemResponse as returned by IOrderItemsAdderService
        private OrderItemResponse CreateOrderItemResponse(OrderItemUpdateRequest request)
        {
            return _fixture.Build<OrderItemResponse>()
                .With(r => r.OrderItemID, Guid.NewGuid())
                .With(r => r.OrderID, request.OrderID)
                .With(r => r.ProductID, request.ProductID)
                .With(r => r.Quantity, request.Quantity)
                .Create();
        }

        #endregion

        #region UpdateOrderItem

        [Fact]
        public async Task UpdateOrderItem_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.UpdateOrderItem(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
            _orderItemsAdderServiceMock.Verify(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderItem_MissingOrderID_ThrowsArgumentException()
        {
            // Arrange – OrderID is required by validation
            var request = CreateValidUpdateRequest(r => r.OrderID = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
            _orderItemsAdderServiceMock.Verify(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderItem_MissingProductID_ThrowsArgumentException()
        {
            // Arrange – ProductID is required by validation
            var request = CreateValidUpdateRequest(r => r.ProductID = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
            _orderItemsAdderServiceMock.Verify(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()), Times.Never);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.345")]
        [InlineData("-5.00")]
        public async Task UpdateOrderItem_InvalidPriceAtSale_ThrowsArgumentException(string price)
        {
            // Arrange – PriceAtSale must match the money regex pattern
            var request = CreateValidUpdateRequest(r => r.PriceAtSale = price);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
            _orderItemsAdderServiceMock.Verify(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderItem_EmptyOrderItemID_CallsAdderServiceInsteadOfRepository()
        {
            // Arrange – OrderItemID == Guid.Empty signals a new item being added to an existing order
            var request = CreateValidUpdateRequest(r => r.OrderItemID = Guid.Empty);
            var addedResponse = CreateOrderItemResponse(request);

            _orderItemsAdderServiceMock
                .Setup(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()))
                .ReturnsAsync(addedResponse);

            // Act
            await _sut.UpdateOrderItem(request);

            // Assert – adder service called, repository update never called
            _orderItemsAdderServiceMock.Verify(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()), Times.Once);
            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderItem_NonEmptyOrderItemID_CallsRepositoryInsteadOfAdderService()
        {
            // Arrange – non-empty OrderItemID signals an existing item to be updated
            var request = CreateValidUpdateRequest();
            var updatedItem = CreateUpdatedOrderItem(request);

            _orderItemsRepositoryMock
                .Setup(r => r.UpdateOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(updatedItem);

            // Act
            await _sut.UpdateOrderItem(request);

            // Assert – repository update called, adder service never called
            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Once);
            _orderItemsAdderServiceMock.Verify(s => s.AddOrderItem(It.IsAny<OrderItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderItem_RepositoryReturnsNull_ThrowsArgumentException()
        {
            // Arrange – repository returns null meaning item was not found
            var request = CreateValidUpdateRequest();

            _orderItemsRepositoryMock
                .Setup(r => r.UpdateOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync((OrderItem?)null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateOrderItem_ValidRequest_MapsFieldsCorrectlyToOrderItemResponse()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            var updatedItem = CreateUpdatedOrderItem(request);

            _orderItemsRepositoryMock
                .Setup(r => r.UpdateOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(updatedItem);

            // Act
            var result = await _sut.UpdateOrderItem(request);

            // Assert – response fields match the updated entity
            result.Should().NotBeNull();
            result.OrderItemID.Should().Be(updatedItem.OrderItemID);
            result.OrderID.Should().Be(updatedItem.OrderID);
            result.ProductID.Should().Be(updatedItem.ProductID);
            result.Quantity.Should().Be(updatedItem.Quantity);
            result.PriceAtSale.Should().Be(updatedItem.PriceAtSale);
        }

        #endregion

        #region UpdateMultipleOrderItems

        [Fact]
        public async Task UpdateMultipleOrderItems_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateMultipleOrderItems(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMultipleOrderItems_EmptyList_ReturnsEmptyList()
        {
            // Arrange – empty list is valid, should return empty response list
            // Act
            var result = await _sut.UpdateMultipleOrderItems(new List<OrderItemUpdateRequest?>());

            // Assert
            result.Should().BeEmpty();
            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMultipleOrderItems_ValidList_ReturnsAllUpdatedItemResponses()
        {
            // Arrange
            var requests = new List<OrderItemUpdateRequest>
        {
            CreateValidUpdateRequest(),
            CreateValidUpdateRequest(),
            CreateValidUpdateRequest(),
        };

            // Each request gets its own updated entity returned by the repository
            foreach (var request in requests)
            {
                var updatedItem = CreateUpdatedOrderItem(request);

                _orderItemsRepositoryMock
                    .Setup(r => r.UpdateOrderItem(It.Is<OrderItem>(i =>
                        i.OrderItemID == request.OrderItemID)))
                    .ReturnsAsync(updatedItem);
            }

            // Act
            var result = await _sut.UpdateMultipleOrderItems(requests.Cast<OrderItemUpdateRequest?>().ToList());

            // Assert
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<OrderItemResponse>();
        }

        [Fact]
        public async Task UpdateMultipleOrderItems_ValidList_CallsRepositoryOncePerItem()
        {
            // Arrange
            var requests = new List<OrderItemUpdateRequest>
        {
            CreateValidUpdateRequest(),
            CreateValidUpdateRequest(),
        };

            foreach (var request in requests)
            {
                var updatedItem = CreateUpdatedOrderItem(request);

                _orderItemsRepositoryMock
                    .Setup(r => r.UpdateOrderItem(It.Is<OrderItem>(i =>
                        i.OrderItemID == request.OrderItemID)))
                    .ReturnsAsync(updatedItem);
            }

            // Act
            await _sut.UpdateMultipleOrderItems(requests.Cast<OrderItemUpdateRequest?>().ToList());

            // Assert – repository called once per item in the list
            _orderItemsRepositoryMock.Verify(r => r.UpdateOrderItem(It.IsAny<OrderItem>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateMultipleOrderItems_ListContainsNullItem_ThrowsArgumentNullException()
        {
            // Arrange – null item inside the list should trigger ArgumentNullException from UpdateOrderItem
            var requests = new List<OrderItemUpdateRequest?>
        {
            CreateValidUpdateRequest(),
            null
        };

            var updatedItem = CreateUpdatedOrderItem(requests[0]!);

            _orderItemsRepositoryMock
                .Setup(r => r.UpdateOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(updatedItem);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateMultipleOrderItems(requests))
                      .Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion
    }
}
