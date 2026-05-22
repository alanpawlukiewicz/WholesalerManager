using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    

    public class OrdersGetterServiceTests
    {
        private readonly Mock<IOrdersRepository> _ordersRepositoryMock;
        private readonly Mock<ILogger<OrdersGetterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrdersGetterService _sut;

        public OrdersGetterServiceTests()
        {
            _ordersRepositoryMock = new Mock<IOrdersRepository>();
            _loggerMock = new Mock<ILogger<OrdersGetterService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrdersGetterService(_ordersRepositoryMock.Object, _loggerMock.Object);
        }

        #region Helpers

        // Creates an Order with a Customer navigation property to ensure ToOrderResponse() maps correctly
        private Order CreateOrder(Action<Order>? configure = null)
        {
            var order = _fixture.Build<Order>()
                .With(o => o.Customer, _fixture.Create<Customer>())
                .Create();

            configure?.Invoke(order);
            return order;
        }

        #endregion

        #region GetAllOrders

        [Fact]
        public async Task GetAllOrders_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no orders
            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _sut.GetAllOrders();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllOrders_WithOrders_ReturnsAllMappedToOrderResponse()
        {
            // Arrange – AutoFixture generates a list of random orders
            var orders = Enumerable.Range(0, 3)
                .Select(_ => CreateOrder())
                .ToList();

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetAllOrders();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<OrderResponse>();
            result.Select(r => r.OrderID)
                  .Should().BeEquivalentTo(orders.Select(o => o.OrderID));
        }

        #endregion

        #region GetOrderByID

        [Fact]
        public async Task GetOrderByID_NullId_ThrowsArgumentNullException()
        {
            // Assert – system should throw exception when ID is null
            await _sut.Invoking(i => i.GetOrderByID(null)).Should().ThrowAsync<ArgumentNullException>();
            _ordersRepositoryMock.Verify(r => r.GetOrderByID(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetOrderByID_NonExistentId_ReturnsNull()
        {
            // Arrange – repository returns null for an unknown ID
            var nonExistentId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(nonExistentId))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _sut.GetOrderByID(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrderByID_ValidId_ReturnsMatchingOrderResponse()
        {
            // Arrange
            var order = CreateOrder();

            _ordersRepositoryMock
                .Setup(r => r.GetOrderByID(order.OrderID))
                .ReturnsAsync(order);

            // Act
            var result = await _sut.GetOrderByID(order.OrderID);

            // Assert – returned DTO matches the source entity
            result.Should().NotBeNull();
            result!.OrderID.Should().Be(order.OrderID);
            result.CustomerID.Should().Be(order.CustomerID);
            result.Status.Should().Be(order.Status);
            result.OrderDate.Should().Be(order.OrderDate);
        }

        #endregion

        #region GetFilteredOrders

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("CustomerName", null)]
        [InlineData("CustomerName", "")]
        [InlineData(null, "filter")]
        public async Task GetFilteredOrders_NullOrEmptyFilterOrProperty_ReturnsAllOrders(
            string? propertyName, string? filter)
        {
            // Arrange – when filter or property name is empty, all orders should be returned
            var orders = Enumerable.Range(0, 3)
                .Select(_ => CreateOrder())
                .ToList();

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetFilteredOrders(propertyName, filter);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetFilteredOrders_FilterByCustomerName_ReturnsMatchingOrders()
        {
            // Arrange – two orders share a common customer name fragment, one does not
            var matching1 = CreateOrder(o => o.Customer = new Customer { CustomerName = "Acme Corp" });
            var matching2 = CreateOrder(o => o.Customer = new Customer { CustomerName = "Acme Ltd" });
            var nonMatching = CreateOrder(o => o.Customer = new Customer { CustomerName = "Other Company" });

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { matching1, matching2, nonMatching });

            // Act – filter by "acme" (case-insensitive by default)
            var result = await _sut.GetFilteredOrders(nameof(OrderResponse.CustomerName), "acme");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(o => o.CustomerName!.Contains("Acme", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredOrders_FilterByTIN_ReturnsMatchingOrders()
        {
            // Arrange
            var matching1 = CreateOrder(o => o.Customer = new Customer { TIN = "PL1234567890" });
            var matching2 = CreateOrder(o => o.Customer = new Customer { TIN = "PL0987654321" });
            var nonMatching = CreateOrder(o => o.Customer = new Customer { TIN = "DE1234567890" });

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredOrders(nameof(OrderResponse.TIN), "PL");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(o => o.TIN!.Contains("PL", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredOrders_FilterByStatus_ReturnsMatchingOrders()
        {
            // Arrange
            var matching1 = CreateOrder(o => o.Status = "PENDING");
            var matching2 = CreateOrder(o => o.Status = "PENDING");
            var nonMatching = CreateOrder(o => o.Status = "COMPLETED");

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredOrders(nameof(OrderResponse.Status), "pending");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(o => o.Status!.Contains("PENDING", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredOrders_FilterByOrderDate_ReturnsOrdersFromThatDay()
        {
            // Arrange – two orders on the target date, one on a different date
            var targetDate = new DateTime(2025, 6, 15, 10, 0, 0);
            var otherDate = new DateTime(2025, 6, 16, 10, 0, 0);

            var matching1 = CreateOrder(o => o.OrderDate = targetDate);
            var matching2 = CreateOrder(o => o.OrderDate = targetDate.AddHours(5));
            var nonMatching = CreateOrder(o => o.OrderDate = otherDate);

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredOrders(nameof(OrderResponse.OrderDate), "2025-06-15");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(o => o.OrderDate!.Value.Date == targetDate.Date);
        }

        [Fact]
        public async Task GetFilteredOrders_FilterByOrderDate_InvalidDateFormat_ReturnsEmptyList()
        {
            // Arrange – unparseable date string should yield an empty result
            var orders = Enumerable.Range(0, 3)
                .Select(_ => CreateOrder())
                .ToList();

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetFilteredOrders(nameof(OrderResponse.OrderDate), "not-a-date");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFilteredOrders_CaseSensitiveFilter_ReturnsOnlyExactCaseMatches()
        {
            // Arrange – same customer name, different casing
            var upperCase = CreateOrder(o => o.Customer = new Customer { CustomerName = "Acme Corp" });
            var lowerCase = CreateOrder(o => o.Customer = new Customer { CustomerName = "acme corp" });

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { upperCase, lowerCase });

            // Act – ignoreCase: false should only match exact casing
            var result = await _sut.GetFilteredOrders(
                nameof(OrderResponse.CustomerName), "Acme Corp", ignoreCase: false);

            // Assert
            result.Should().HaveCount(1);
            result.First().CustomerName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task GetFilteredOrders_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { CreateOrder() });

            // Act & Assert – an unsupported property name should throw ArgumentException
            await _sut.Invoking(s => s.GetFilteredOrders("InvalidProperty", "someFilter"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*InvalidProperty*");
        }

        #endregion

        #region GetSortedOrders

        [Fact]
        public async Task GetSortedOrders_NullPropertyName_ReturnsOrdersInOriginalOrder()
        {
            // Arrange – without a sort property, original repository order should be preserved
            var orders = Enumerable.Range(0, 3)
                .Select(_ => CreateOrder())
                .ToList();

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetSortedOrders(null);

            // Assert – IDs match original order
            result.Select(r => r.OrderID)
                  .Should().ContainInOrder(orders.Select(o => o.OrderID));
        }

        [Fact]
        public async Task GetSortedOrders_ByCustomerNameAscending_ReturnsSortedAscending()
        {
            // Arrange
            var orders = new List<Order>
        {
            CreateOrder(o => o.Customer = new Customer { CustomerName = "Zebra Inc" }),
            CreateOrder(o => o.Customer = new Customer { CustomerName = "Acme Corp" }),
            CreateOrder(o => o.Customer = new Customer { CustomerName = "Mango Ltd" }),
        };

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetSortedOrders(nameof(OrderResponse.CustomerName), SortOrderOptions.ASC);

            // Assert
            result.Select(o => o.CustomerName)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedOrders_ByCustomerNameDescending_ReturnsSortedDescending()
        {
            // Arrange
            var orders = new List<Order>
        {
            CreateOrder(o => o.Customer = new Customer { CustomerName = "Acme Corp" }),
            CreateOrder(o => o.Customer = new Customer { CustomerName = "Zebra Inc" }),
            CreateOrder(o => o.Customer = new Customer { CustomerName = "Mango Ltd" }),
        };

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetSortedOrders(nameof(OrderResponse.CustomerName), SortOrderOptions.DESC);

            // Assert
            result.Select(o => o.CustomerName)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedOrders_ByOrderDateAscending_ReturnsSortedByDateAscending()
        {
            // Arrange – dates set explicitly so sort order is deterministic
            var orders = new List<Order>
        {
            CreateOrder(o => o.OrderDate = new DateTime(2025, 3, 1)),
            CreateOrder(o => o.OrderDate = new DateTime(2025, 1, 1)),
            CreateOrder(o => o.OrderDate = new DateTime(2025, 2, 1)),
        };

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetSortedOrders(nameof(OrderResponse.OrderDate), SortOrderOptions.ASC);

            // Assert
            result.Select(o => o.OrderDate)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedOrders_ByOrderDateDescending_ReturnsSortedByDateDescending()
        {
            // Arrange
            var orders = new List<Order>
        {
            CreateOrder(o => o.OrderDate = new DateTime(2025, 1, 1)),
            CreateOrder(o => o.OrderDate = new DateTime(2025, 3, 1)),
            CreateOrder(o => o.OrderDate = new DateTime(2025, 2, 1)),
        };

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetSortedOrders(nameof(OrderResponse.OrderDate), SortOrderOptions.DESC);

            // Assert
            result.Select(o => o.OrderDate)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedOrders_ByStatusAscending_ReturnsSortedByStatusAscending()
        {
            // Arrange
            var orders = new List<Order>
        {
            CreateOrder(o => o.Status = "PENDING"),
            CreateOrder(o => o.Status = "COMPLETED"),
            CreateOrder(o => o.Status = "CANCELLED"),
        };

            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetSortedOrders(nameof(OrderResponse.Status), SortOrderOptions.ASC);

            // Assert
            result.Select(o => o.Status)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedOrders_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _ordersRepositoryMock
                .Setup(r => r.GetAllOrders())
                .ReturnsAsync(new List<Order> { CreateOrder() });

            // Act & Assert
            await _sut.Invoking(s => s.GetSortedOrders("NonExistentProperty"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*NonExistentProperty*");
        }

        #endregion
    }
}
