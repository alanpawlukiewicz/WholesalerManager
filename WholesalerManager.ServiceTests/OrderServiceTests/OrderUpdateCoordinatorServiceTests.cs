using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderServices;

namespace WholesalerManager.ServiceTests.OrderServiceTests
{
    public class OrderUpdateCoordinatorServiceTests
    {
        private readonly Mock<IOrdersUpdaterService> _ordersUpdaterServiceMock;
        private readonly Mock<IOrderItemsUpdaterService> _orderItemsUpdaterServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<OrderUpdateCoordinatorService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrderUpdateCoordinatorService _sut;

        public OrderUpdateCoordinatorServiceTests()
        {
            _ordersUpdaterServiceMock = new Mock<IOrdersUpdaterService>();
            _orderItemsUpdaterServiceMock = new Mock<IOrderItemsUpdaterService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OrderUpdateCoordinatorService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrderUpdateCoordinatorService(
                _ordersUpdaterServiceMock.Object,
                _orderItemsUpdaterServiceMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid OrderUpdateRequest with all required fields filled
        private OrderUpdateRequest CreateValidOrderUpdateRequest(Action<OrderUpdateRequest>? configure = null)
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

        // Creates a list of valid OrderItemUpdateRequests
        private List<OrderItemUpdateRequest> CreateValidOrderItemUpdateRequests(int count = 2)
        {
            return _fixture.Build<OrderItemUpdateRequest>()
                .With(i => i.OrderID, Guid.NewGuid())
                .With(i => i.ProductID, Guid.NewGuid())
                .With(i => i.PriceAtSale, "19.99")
                .CreateMany(count)
                .ToList();
        }

        // Sets up all mocks for a successful full order update flow
        private void SetupSuccessfulUpdate(OrderUpdateRequest request, OrderResponse orderResponse)
        {
            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrder(request))
                .ReturnsAsync(orderResponse);

            _orderItemsUpdaterServiceMock
                .Setup(s => s.UpdateMultipleOrderItems(It.IsAny<List<OrderItemUpdateRequest?>>()))
                .ReturnsAsync(_fixture.CreateMany<OrderItemResponse>().ToList());
        }

        #endregion

        #region UpdateFullOrder

        [Fact]
        public async Task UpdateFullOrder_NullOrderRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var items = CreateValidOrderItemUpdateRequests();

            // Act & Assert – null order request should throw before any service call
            await _sut.Invoking(s => s.UpdateFullOrder(null, items))
                      .Should().ThrowAsync<ArgumentNullException>();

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _ordersUpdaterServiceMock.Verify(s => s.UpdateOrder(It.IsAny<OrderUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFullOrder_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            var orderRequest = CreateValidOrderUpdateRequest();

            // Act & Assert – null items list should throw before any service call
            await _sut.Invoking(s => s.UpdateFullOrder(orderRequest, null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _ordersUpdaterServiceMock.Verify(s => s.UpdateOrder(It.IsAny<OrderUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFullOrder_ValidRequest_BeginsTransaction()
        {
            // Arrange
            var orderRequest = CreateValidOrderUpdateRequest();
            var items = CreateValidOrderItemUpdateRequests();
            var orderResponse = _fixture.Create<OrderResponse>();

            SetupSuccessfulUpdate(orderRequest, orderResponse);

            // Act
            await _sut.UpdateFullOrder(orderRequest, items);

            // Assert – transaction must be started before any work is done
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateFullOrder_ValidRequest_CommitsTransactionAndReturnsOrderResponse()
        {
            // Arrange
            var orderRequest = CreateValidOrderUpdateRequest();
            var items = CreateValidOrderItemUpdateRequests();
            var orderResponse = _fixture.Create<OrderResponse>();

            SetupSuccessfulUpdate(orderRequest, orderResponse);

            // Act
            var result = await _sut.UpdateFullOrder(orderRequest, items);

            // Assert – transaction committed, rollback never called, correct response returned
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
            result.Should().BeEquivalentTo(orderResponse);
        }

        [Fact]
        public async Task UpdateFullOrder_ValidRequest_CallsUpdateOrderAndUpdateMultipleOrderItemsOnce()
        {
            // Arrange
            var orderRequest = CreateValidOrderUpdateRequest();
            var items = CreateValidOrderItemUpdateRequests();
            var orderResponse = _fixture.Create<OrderResponse>();

            SetupSuccessfulUpdate(orderRequest, orderResponse);

            // Act
            await _sut.UpdateFullOrder(orderRequest, items);

            // Assert – both inner services called exactly once
            _ordersUpdaterServiceMock.Verify(s => s.UpdateOrder(orderRequest), Times.Once);
            _orderItemsUpdaterServiceMock.Verify(
                s => s.UpdateMultipleOrderItems(items), Times.Once);
        }

        [Fact]
        public async Task UpdateFullOrder_UpdateOrderThrowsException_RollsBackTransactionAndRethrows()
        {
            // Arrange
            var orderRequest = CreateValidOrderUpdateRequest();
            var items = CreateValidOrderItemUpdateRequests();

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Simulate failure in UpdateOrder
            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrder(orderRequest))
                .ThrowsAsync(new Exception("UpdateOrder failed"));

            // Act & Assert – exception should propagate after rollback
            await _sut.Invoking(s => s.UpdateFullOrder(orderRequest, items))
                      .Should().ThrowAsync<Exception>()
                      .WithMessage("UpdateOrder failed");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateFullOrder_UpdateMultipleOrderItemsThrowsException_RollsBackTransactionAndRethrows()
        {
            // Arrange
            var orderRequest = CreateValidOrderUpdateRequest();
            var items = CreateValidOrderItemUpdateRequests();
            var orderResponse = _fixture.Create<OrderResponse>();

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrder(orderRequest))
                .ReturnsAsync(orderResponse);

            // Simulate failure in UpdateMultipleOrderItems
            _orderItemsUpdaterServiceMock
                .Setup(s => s.UpdateMultipleOrderItems(It.IsAny<List<OrderItemUpdateRequest?>>()))
                .ThrowsAsync(new Exception("UpdateMultipleOrderItems failed"));

            // Act & Assert
            await _sut.Invoking(s => s.UpdateFullOrder(orderRequest, items))
                      .Should().ThrowAsync<Exception>()
                      .WithMessage("UpdateMultipleOrderItems failed");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        #endregion
    }
}
