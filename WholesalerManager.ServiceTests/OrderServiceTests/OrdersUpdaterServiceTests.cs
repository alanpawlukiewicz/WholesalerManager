using AutoFixture;
using FluentAssertions;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderServices;
using Xunit;

namespace WholesalerManager.ServiceTests.OrderServiceTests
{
    

    public class OrdersUpdaterServiceTests
    {
        private readonly Mock<IOrdersRepository> _ordersRepositoryMock;
        private readonly Mock<IOrdersStockCheckerService> _ordersStockCheckerServiceMock;
        private readonly IFixture _fixture;
        private readonly IOrdersUpdaterService _sut;

        public OrdersUpdaterServiceTests()
        {
            _ordersRepositoryMock = new Mock<IOrdersRepository>();
            _ordersStockCheckerServiceMock = new Mock<IOrdersStockCheckerService>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrdersUpdaterService(
                _ordersRepositoryMock.Object,
                _ordersStockCheckerServiceMock.Object
            );
        }

        #region Helpers

        // Creates a valid OrderUpdateRequest with all required fields filled
        private OrderUpdateRequest CreateValidUpdateRequest(Action<OrderUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<OrderUpdateRequest>()
                .With(r => r.OrderID, Guid.NewGuid())
                .With(r => r.CustomerID, Guid.NewGuid())
                .With(r => r.OrderDate, DateTime.UtcNow)
                .With(r => r.Status, OrderStatus.PENDING)
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates an Order entity with a Customer navigation property
        private Order CreateOrder(Action<Order>? configure = null)
        {
            var order = _fixture.Build<Order>()
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            configure?.Invoke(order);
            return order;
        }

        #endregion

        #region CancelOrder

        [Fact]
        public async Task CancelOrder_EmptyGuid_ThrowsArgumentException()
        {
            // Act & Assert – Guid.Empty is explicitly rejected
            await _sut.Invoking(s => s.CancelOrder(Guid.Empty))
                      .Should().ThrowAsync<ArgumentException>();

            _ordersRepositoryMock.Verify(r => r.GetOrderByID(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_NonExistentOrder_ReturnsFalse()
        {
            // Arrange – repository returns null for the given ID
            var orderId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _sut.CancelOrder(orderId);

            // Assert
            result.Should().BeFalse();
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_DeliveredOrder_ReturnsFalse()
        {
            // Arrange – delivered orders cannot be cancelled
            var orderId = _fixture.Create<Guid>();
            var order = CreateOrder(o =>
            {
                o.OrderID = orderId;
                o.Status = OrderStatus.DELIVERED.ToString();
            });

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _sut.CancelOrder(orderId);

            // Assert
            result.Should().BeFalse();
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Theory]
        [InlineData("PENDING")]
        [InlineData("PAID")]
        [InlineData("PROCESSING")]
        [InlineData("SHIPPED")]
        public async Task CancelOrder_NonDeliveredOrder_ReturnsTrueAndSetsStatusToCancelled(string initialStatus)
        {
            // Arrange – any non-delivered status should allow cancellation
            var orderId = _fixture.Create<Guid>();
            var order = CreateOrder(o =>
            {
                o.OrderID = orderId;
                o.Status = initialStatus;
            });

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _ordersRepositoryMock
                .Setup(r => r.Save())
                .ReturnsAsync(It.IsAny<int>);

            // Act
            var result = await _sut.CancelOrder(orderId);

            // Assert – order status updated and changes persisted
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.CANCELLED.ToString());
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region UpdateOrder

        [Fact]
        public async Task UpdateOrder_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrder(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _ordersRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_MissingCustomerID_ThrowsArgumentException()
        {
            // Arrange – CustomerID is required by validation
            var request = CreateValidUpdateRequest(r => r.CustomerID = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrder(request))
                      .Should().ThrowAsync<ArgumentException>();

            _ordersRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_MissingOrderDate_ThrowsArgumentException()
        {
            // Arrange – OrderDate is required by validation
            var request = CreateValidUpdateRequest(r => r.OrderDate = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrder(request))
                      .Should().ThrowAsync<ArgumentException>();

            _ordersRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Theory]
        [InlineData(OrderStatus.PAID)]
        [InlineData(OrderStatus.PROCESSING)]
        public async Task UpdateOrder_StatusRequiresStockCheck_InsufficientStock_ThrowsInvalidOperationException(
            OrderStatus status)
        {
            // Arrange – stock check fails for PAID and PROCESSING statuses
            var request = CreateValidUpdateRequest(r => r.Status = status);

            _ordersStockCheckerServiceMock
                .Setup(s => s.CheckStockAvailabilityForOrder(request.OrderID))
                .ReturnsAsync(false);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrder(request))
                      .Should().ThrowAsync<InvalidOperationException>()
                      .WithMessage("*stock*");

            _ordersRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Theory]
        [InlineData(OrderStatus.PAID)]
        [InlineData(OrderStatus.PROCESSING)]
        public async Task UpdateOrder_StatusRequiresStockCheck_SufficientStock_CallsRepositoryUpdate(
            OrderStatus status)
        {
            // Arrange – stock check passes for PAID and PROCESSING statuses
            var request = CreateValidUpdateRequest(r => r.Status = status);
            var updatedOrder = CreateOrder(o => o.OrderID = request.OrderID);

            _ordersStockCheckerServiceMock
                .Setup(s => s.CheckStockAvailabilityForOrder(request.OrderID))
                .ReturnsAsync(true);

            _ordersRepositoryMock
                .Setup(r => r.UpdateOrder(It.IsAny<Order>()))
                .ReturnsAsync(updatedOrder);

            // Act
            await _sut.UpdateOrder(request);

            // Assert – repository update called after successful stock check
            _ordersRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Once);
        }

        [Theory]
        [InlineData(OrderStatus.PENDING)]
        [InlineData(OrderStatus.SHIPPED)]
        [InlineData(OrderStatus.DELIVERED)]
        [InlineData(OrderStatus.CANCELLED)]
        public async Task UpdateOrder_StatusNotRequiringStockCheck_DoesNotCallStockChecker(
            OrderStatus status)
        {
            // Arrange – statuses other than PAID/PROCESSING skip the stock check
            var request = CreateValidUpdateRequest(r => r.Status = status);
            var updatedOrder = CreateOrder(o => o.OrderID = request.OrderID);

            _ordersRepositoryMock
                .Setup(r => r.UpdateOrder(It.IsAny<Order>()))
                .ReturnsAsync(updatedOrder);

            // Act
            await _sut.UpdateOrder(request);

            // Assert – stock checker should never be called for these statuses
            _ordersStockCheckerServiceMock.Verify(
                s => s.CheckStockAvailabilityForOrder(It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrder_RepositoryReturnsNull_ThrowsArgumentException()
        {
            // Arrange – repository returns null after update, meaning order was not found
            var request = CreateValidUpdateRequest(r => r.Status = OrderStatus.PENDING);

            _ordersRepositoryMock
                .Setup(r => r.UpdateOrder(It.IsAny<Order>()))
                .ReturnsAsync((Order?)null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateOrder(request))
                      .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateOrder_ValidRequest_ReturnsOrderResponse()
        {
            // Arrange
            var request = CreateValidUpdateRequest(r => r.Status = OrderStatus.PENDING);
            var updatedOrder = CreateOrder(o =>
            {
                o.OrderID = request.OrderID;
                o.CustomerID = request.CustomerID;
                o.OrderDate = request.OrderDate;
                o.Status = request.Status.ToString();
            });

            _ordersRepositoryMock
                .Setup(r => r.UpdateOrder(It.IsAny<Order>()))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _sut.UpdateOrder(request);

            // Assert – response reflects the updated order entity
            result.Should().NotBeNull();
            result.OrderID.Should().Be(updatedOrder.OrderID);
            result.CustomerID.Should().Be(updatedOrder.CustomerID);
            result.OrderDate.Should().Be(updatedOrder.OrderDate);
            result.Status.Should().Be(updatedOrder.Status);
        }

        #endregion

        #region UpdateOrderStatus

        [Fact]
        public async Task UpdateOrderStatus_EmptyGuid_ThrowsArgumentException()
        {
            // Act & Assert – Guid.Empty is explicitly rejected
            await _sut.Invoking(s => s.UpdateOrderStatus(Guid.Empty, OrderStatus.PAID))
                      .Should().ThrowAsync<ArgumentException>();

            _ordersRepositoryMock.Verify(r => r.GetOrderByID(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatus_NonExistentOrder_ReturnsFalse()
        {
            // Arrange – repository returns null for the given ID
            var orderId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.PAID);

            // Assert
            result.Should().BeFalse();
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Theory]
        [InlineData(OrderStatus.PENDING)]
        [InlineData(OrderStatus.DELIVERED)]
        [InlineData(OrderStatus.CANCELLED)]
        [InlineData(OrderStatus.RETURNED)]
        public async Task UpdateOrderStatus_ForbiddenTargetStatus_ReturnsFalse(OrderStatus forbiddenStatus)
        {
            // Arrange – these statuses cannot be set via UpdateOrderStatus
            var orderId = _fixture.Create<Guid>();
            var order = CreateOrder(o => o.OrderID = orderId);

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _sut.UpdateOrderStatus(orderId, forbiddenStatus);

            // Assert
            result.Should().BeFalse();
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Theory]
        [InlineData("PENDING", OrderStatus.PROCESSING)]  // must go PENDING -> PAID first
        [InlineData("PENDING", OrderStatus.SHIPPED)]
        [InlineData("PAID", OrderStatus.SHIPPED)]         // must go PAID -> PROCESSING first
        [InlineData("PROCESSING", OrderStatus.PAID)]      // cannot go backwards
        public async Task UpdateOrderStatus_InvalidTransition_ReturnsFalse(
            string currentStatus, OrderStatus targetStatus)
        {
            // Arrange – status transitions must follow the defined order
            var orderId = _fixture.Create<Guid>();
            var order = CreateOrder(o =>
            {
                o.OrderID = orderId;
                o.Status = currentStatus;
            });

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _sut.UpdateOrderStatus(orderId, targetStatus);

            // Assert
            result.Should().BeFalse();
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Theory]
        [InlineData("PENDING", OrderStatus.PAID)]
        [InlineData("PAID", OrderStatus.PROCESSING)]
        [InlineData("PROCESSING", OrderStatus.SHIPPED)]
        public async Task UpdateOrderStatus_ValidTransition_ReturnsTrueAndSavesChanges(
            string currentStatus, OrderStatus targetStatus)
        {
            // Arrange – valid transitions follow the defined status flow
            var orderId = _fixture.Create<Guid>();
            var order = CreateOrder(o =>
            {
                o.OrderID = orderId;
                o.Status = currentStatus;
            });

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(orderId))
                .ReturnsAsync(order);

            _ordersRepositoryMock
                .Setup(r => r.Save())
                .ReturnsAsync(It.IsAny<int>);

            // Act
            var result = await _sut.UpdateOrderStatus(orderId, targetStatus);

            // Assert – status updated and changes persisted
            result.Should().BeTrue();
            order.Status.Should().Be(targetStatus.ToString());
            _ordersRepositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion
    }
}
