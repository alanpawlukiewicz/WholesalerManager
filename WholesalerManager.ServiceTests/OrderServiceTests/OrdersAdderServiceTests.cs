using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderServices;
using Xunit;

namespace WholesalerManager.ServiceTests.OrderServiceTests
{
    

    public class OrdersAdderServiceTests
    {
        private readonly Mock<IOrdersRepository> _ordersRepositoryMock;
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<ILogger<OrdersAdderService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrdersAdderService _sut;

        public OrdersAdderServiceTests()
        {
            _ordersRepositoryMock = new Mock<IOrdersRepository>();
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _loggerMock = new Mock<ILogger<OrdersAdderService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrdersAdderService(
                _ordersRepositoryMock.Object,
                _customersGetterServiceMock.Object,
                _loggerMock.Object
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

        // Creates an Order entity that the repository will return after adding
        private Order CreateAddedOrder(OrderAddRequest request)
        {
            return _fixture.Build<Order>()
                .With(o => o.CustomerID, request.CustomerID)
                .With(o => o.OrderDate, request.OrderDate)
                .With(o => o.Status, request.Status?.ToString())
                .With(o => o.Customer, _fixture.Build<Customer>()
                    .With(c => c.CustomerID, request.CustomerID!.Value)
                    .Create())
                .Create();
        }

        #endregion

        #region AddOrder

        [Fact]
        public async Task AddOrder_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any service or repository call
            await _sut.Invoking(s => s.AddOrder(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByID(It.IsAny<Guid?>()), Times.Never);
            _ordersRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task AddOrder_MissingCustomerID_ThrowsArgumentException()
        {
            // Arrange – CustomerID is required by validation
            var request = CreateValidOrderRequest(r => r.CustomerID = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrder(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByID(It.IsAny<Guid?>()), Times.Never);
            _ordersRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task AddOrder_MissingOrderDate_ThrowsArgumentException()
        {
            // Arrange – OrderDate is required by validation
            var request = CreateValidOrderRequest(r => r.OrderDate = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrder(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByID(It.IsAny<Guid?>()), Times.Never);
            _ordersRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task AddOrder_NonExistentCustomer_ThrowsArgumentException()
        {
            // Arrange – customer lookup returns null, meaning customer does not exist
            var request = CreateValidOrderRequest();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(request.CustomerID))
                .ReturnsAsync((CustomerResponse?)null);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrder(request))
                      .Should().ThrowAsync<ArgumentException>();

            _ordersRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task AddOrder_ValidRequest_ReturnsOrderResponseWithNewGuid()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            var customerResponse = _fixture.Create<CustomerResponse>();
            var addedOrder = CreateAddedOrder(request);

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(request.CustomerID))
                .ReturnsAsync(customerResponse);

            _ordersRepositoryMock
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(addedOrder);

            // Act
            var result = await _sut.AddOrder(request);

            // Assert – service should assign a new non-empty OrderID
            result.Should().NotBeNull();
            result.OrderID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddOrder_ValidRequest_MapsFieldsCorrectlyToOrderResponse()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            var customerResponse = _fixture.Create<CustomerResponse>();
            var addedOrder = CreateAddedOrder(request);

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(request.CustomerID))
                .ReturnsAsync(customerResponse);

            _ordersRepositoryMock
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(addedOrder);

            // Act
            var result = await _sut.AddOrder(request);

            // Assert – response fields match the added order entity
            result.CustomerID.Should().Be(addedOrder.CustomerID);
            result.OrderDate.Should().Be(addedOrder.OrderDate);
            result.Status.Should().Be(addedOrder.Status);
        }

        [Fact]
        public async Task AddOrder_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            var customerResponse = _fixture.Create<CustomerResponse>();
            var addedOrder = CreateAddedOrder(request);

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(request.CustomerID))
                .ReturnsAsync(customerResponse);

            _ordersRepositoryMock
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(addedOrder);

            // Act
            await _sut.AddOrder(request);

            // Assert
            _ordersRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task AddOrder_ValidRequest_PassesOrderWithNewGuidToRepository()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            var customerResponse = _fixture.Create<CustomerResponse>();
            var addedOrder = CreateAddedOrder(request);
            Order? capturedOrder = null;

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(request.CustomerID))
                .ReturnsAsync(customerResponse);

            // Capture the order passed to the repository to verify its contents
            _ordersRepositoryMock
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o)
                .ReturnsAsync(addedOrder);

            // Act
            await _sut.AddOrder(request);

            // Assert – the order passed to the repository should have a new non-empty ID
            capturedOrder.Should().NotBeNull();
            capturedOrder!.OrderID.Should().NotBeEmpty();
            capturedOrder.CustomerID.Should().Be(request.CustomerID);
            capturedOrder.OrderDate.Should().Be(request.OrderDate);
        }

        #endregion
    }
}
