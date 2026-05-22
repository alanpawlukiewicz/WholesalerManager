using AutoFixture;
using FluentAssertions;
using Moq;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderServices;
using Xunit;

namespace WholesalerManager.ServiceTests.OrderServiceTests
{
    public class OrderRegistrationServiceTests
    {
        private readonly Mock<IOrdersAdderService> _ordersAdderServiceMock;
        private readonly Mock<IOrderItemsAdderService> _orderItemsAdderServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IFixture _fixture;
        private readonly IOrderRegistrationService _sut;

        public OrderRegistrationServiceTests()
        {
            _ordersAdderServiceMock = new Mock<IOrdersAdderService>();
            _orderItemsAdderServiceMock = new Mock<IOrderItemsAdderService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrderRegistrationService(
                _ordersAdderServiceMock.Object,
                _orderItemsAdderServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        #region Helpers

        // Creates a valid OrderAddRequest with all required fields filled
        private OrderAddRequest CreateValidOrderRequest(Action<OrderAddRequest>? configure = null)
        {
            var request = _fixture.Build<OrderAddRequest>()
                .With(r => r.CustomerID, Guid.NewGuid())
                .With(r => r.OrderDate, DateTime.UtcNow)
                .With(r => r.Status, OrderStatus.PENDING)
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a list of valid OrderItemAddRequests
        private List<OrderItemAddRequest> CreateValidOrderItems(int count = 2)
        {
            return _fixture.Build<OrderItemAddRequest>()
                .With(i => i.ProductID, Guid.NewGuid())
                .With(i => i.Quantity, _fixture.Create<int>())
                .With(i => i.PriceAtSale, "19.99")
                .CreateMany(count)
                .ToList();
        }

        // Creates an OrderResponse as returned by IOrdersAdderService
        private OrderResponse CreateOrderResponse(Guid? orderId = null)
        {
            return _fixture.Build<OrderResponse>()
                .With(r => r.OrderID, orderId ?? Guid.NewGuid())
                .Create();
        }

        #endregion

        #region RegisterFullOrder

        [Fact]
        public async Task RegisterFullOrder_NullOrderRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var items = CreateValidOrderItems();

            // Act & Assert – null order request should throw before any service call
            await _sut.Invoking(s => s.RegisterFullOrder(null, items))
                      .Should().ThrowAsync<ArgumentNullException>();

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _ordersAdderServiceMock.Verify(s => s.AddOrder(It.IsAny<OrderAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task RegisterFullOrder_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();

            // Act & Assert – null items list should throw before any service call
            await _sut.Invoking(s => s.RegisterFullOrder(orderRequest, null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _ordersAdderServiceMock.Verify(s => s.AddOrder(It.IsAny<OrderAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task RegisterFullOrder_ValidRequest_BeginsTransaction()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();
            var items = CreateValidOrderItems();
            var orderResponse = CreateOrderResponse();

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(orderRequest))
                .ReturnsAsync(orderResponse);

            _orderItemsAdderServiceMock
                .Setup(s => s.AddMultipleOrderItems(It.IsAny<List<OrderItemAddRequest>>()))
                .ReturnsAsync(It.IsAny<List<OrderItemResponse>>);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RegisterFullOrder(orderRequest, items);

            // Assert – transaction must be started before any work is done
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterFullOrder_ValidRequest_CommitsTransactionAndReturnsOrderResponse()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();
            var items = CreateValidOrderItems();
            var orderResponse = CreateOrderResponse();

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(orderRequest))
                .ReturnsAsync(orderResponse);

            _orderItemsAdderServiceMock
                .Setup(s => s.AddMultipleOrderItems(It.IsAny<List<OrderItemAddRequest>>()))
                .ReturnsAsync(It.IsAny<List<OrderItemResponse>>);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RegisterFullOrder(orderRequest, items);

            // Assert – transaction committed and correct order returned
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
            result.Should().BeEquivalentTo(orderResponse);
        }

        [Fact]
        public async Task RegisterFullOrder_ValidRequest_AssignsOrderIdToAllItems()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();
            var items = CreateValidOrderItems(3);
            var orderResponse = CreateOrderResponse();

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(orderRequest))
                .ReturnsAsync(orderResponse);

            _orderItemsAdderServiceMock
                .Setup(s => s.AddMultipleOrderItems(It.IsAny<List<OrderItemAddRequest>>()))
                .ReturnsAsync(It.IsAny<List<OrderItemResponse>>);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.RegisterFullOrder(orderRequest, items);

            // Assert – all items should have the OrderID assigned from the created order
            items.Should().OnlyContain(i => i.OrderID == orderResponse.OrderID);
        }

        [Fact]
        public async Task RegisterFullOrder_ValidRequest_CallsAddOrderAndAddMultipleOrderItemsOnce()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();
            var items = CreateValidOrderItems();
            var orderResponse = CreateOrderResponse();

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(orderRequest))
                .ReturnsAsync(orderResponse);

            _orderItemsAdderServiceMock
                .Setup(s => s.AddMultipleOrderItems(It.IsAny<List<OrderItemAddRequest>>()))
                .ReturnsAsync(It.IsAny<List<OrderItemResponse>>);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.RegisterFullOrder(orderRequest, items);

            // Assert
            _ordersAdderServiceMock.Verify(s => s.AddOrder(orderRequest), Times.Once);
            _orderItemsAdderServiceMock.Verify(s => s.AddMultipleOrderItems(items), Times.Once);
        }

        [Fact]
        public async Task RegisterFullOrder_AddOrderThrowsException_RollsBackTransactionAndRethrows()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();
            var items = CreateValidOrderItems();

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Simulate failure in AddOrder
            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(orderRequest))
                .ThrowsAsync(new Exception("AddOrder failed"));

            // Act & Assert – exception should propagate after rollback
            await _sut.Invoking(s => s.RegisterFullOrder(orderRequest, items))
                      .Should().ThrowAsync<Exception>()
                      .WithMessage("AddOrder failed");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterFullOrder_AddMultipleOrderItemsThrowsException_RollsBackTransactionAndRethrows()
        {
            // Arrange
            var orderRequest = CreateValidOrderRequest();
            var items = CreateValidOrderItems();
            var orderResponse = CreateOrderResponse();

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(orderRequest))
                .ReturnsAsync(orderResponse);

            // Simulate failure in AddMultipleOrderItems
            _orderItemsAdderServiceMock
                .Setup(s => s.AddMultipleOrderItems(It.IsAny<List<OrderItemAddRequest>>()))
                .ThrowsAsync(new Exception("AddMultipleOrderItems failed"));

            // Act & Assert
            await _sut.Invoking(s => s.RegisterFullOrder(orderRequest, items))
                      .Should().ThrowAsync<Exception>()
                      .WithMessage("AddMultipleOrderItems failed");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        #endregion
    }
}
