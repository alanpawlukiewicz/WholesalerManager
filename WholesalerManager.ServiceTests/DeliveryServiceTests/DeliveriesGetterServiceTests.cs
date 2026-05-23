using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;

namespace WholesalerManager.ServiceTests.DeliveryServiceTests
{


    public class DeliveriesGetterServiceTests
    {
        private readonly Mock<IDeliveriesRepository> _deliveriesRepositoryMock;
        private readonly Mock<ILogger<DeliveriesGetterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveriesGetterService _sut;

        public DeliveriesGetterServiceTests()
        {
            _deliveriesRepositoryMock = new Mock<IDeliveriesRepository>();
            _loggerMock = new Mock<ILogger<DeliveriesGetterService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveriesGetterService(
                _deliveriesRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a Delivery with a Supplier navigation property to ensure ToDeliveryResponse() maps correctly
        private Delivery CreateDelivery(Action<Delivery>? configure = null)
        {
            var delivery = _fixture.Build<Delivery>()
                .With(d => d.Supplier, _fixture.Create<Supplier>())
                .Create();

            configure?.Invoke(delivery);
            return delivery;
        }

        #endregion

        #region GetAllDeliveries

        [Fact]
        public async Task GetAllDeliveries_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no deliveries
            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery>());

            // Act
            var result = await _sut.GetAllDeliveries();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllDeliveries_WithDeliveries_ReturnsAllMappedToDeliveryResponse()
        {
            // Arrange – AutoFixture generates a list of random deliveries
            var deliveries = Enumerable.Range(0, 3)
                .Select(_ => CreateDelivery())
                .ToList();

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetAllDeliveries();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<DeliveryResponse>();
            result.Select(r => r.DeliveryID)
                  .Should().BeEquivalentTo(deliveries.Select(d => d.DeliveryID));
        }

        [Fact]
        public async Task GetAllDeliveries_WithDeliveries_MapsFieldsCorrectly()
        {
            // Arrange – single delivery to verify field mapping precisely
            var delivery = CreateDelivery();

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { delivery });

            // Act
            var result = await _sut.GetAllDeliveries();

            // Assert
            var response = result.Single();
            response.DeliveryID.Should().Be(delivery.DeliveryID);
            response.SupplierID.Should().Be(delivery.SupplierID);
            response.SupplierName.Should().Be(delivery.Supplier?.SupplierName);
            response.OrderDate.Should().Be(delivery.OrderDate);
            response.Status.Should().Be(delivery.Status);
        }

        #endregion

        #region GetDeliveryById

        [Fact]
        public async Task GetDeliveryById_NullId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.GetDeliveryById(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveriesRepositoryMock.Verify(r => r.GetDeliveryById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetDeliveryById_NonExistentId_ReturnsNull()
        {
            // Arrange – repository returns null for an unknown ID
            var nonExistentId = _fixture.Create<Guid>();

            _deliveriesRepositoryMock
                .Setup(r => r.GetDeliveryById(nonExistentId))
                .ReturnsAsync((Delivery?)null);

            // Act
            var result = await _sut.GetDeliveryById(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDeliveryById_ValidId_ReturnsMatchingDeliveryResponse()
        {
            // Arrange
            var delivery = CreateDelivery();

            _deliveriesRepositoryMock
                .Setup(r => r.GetDeliveryById(delivery.DeliveryID))
                .ReturnsAsync(delivery);

            // Act
            var result = await _sut.GetDeliveryById(delivery.DeliveryID);

            // Assert – returned DTO matches the source entity
            result.Should().NotBeNull();
            result!.DeliveryID.Should().Be(delivery.DeliveryID);
            result.SupplierID.Should().Be(delivery.SupplierID);
            result.SupplierName.Should().Be(delivery.Supplier?.SupplierName);
            result.OrderDate.Should().Be(delivery.OrderDate);
            result.Status.Should().Be(delivery.Status);
        }

        [Fact]
        public async Task GetDeliveryById_ValidId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var delivery = CreateDelivery();

            _deliveriesRepositoryMock
                .Setup(r => r.GetDeliveryById(delivery.DeliveryID))
                .ReturnsAsync(delivery);

            // Act
            await _sut.GetDeliveryById(delivery.DeliveryID);

            // Assert
            _deliveriesRepositoryMock.Verify(r => r.GetDeliveryById(delivery.DeliveryID), Times.Once);
        }

        #endregion

        #region GetFilteredDeliveries

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("SupplierName", null)]
        [InlineData("SupplierName", "")]
        [InlineData(null, "filter")]
        public async Task GetFilteredDeliveries_NullOrEmptyFilterOrProperty_ReturnsAllDeliveries(
            string? propertyName, string? filter)
        {
            // Arrange – when filter or property name is empty, all deliveries should be returned
            var deliveries = Enumerable.Range(0, 3)
                .Select(_ => CreateDelivery())
                .ToList();

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetFilteredDeliveries(propertyName, filter);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetFilteredDeliveries_FilterBySupplierName_ReturnsMatchingDeliveries()
        {
            // Arrange – two deliveries share a common supplier name fragment, one does not
            var matching1 = CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Acme Supplies" });
            var matching2 = CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Acme Logistics" });
            var nonMatching = CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Other Supplier" });

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { matching1, matching2, nonMatching });

            // Act – filter by "acme" (case-insensitive by default)
            var result = await _sut.GetFilteredDeliveries(nameof(DeliveryResponse.SupplierName), "acme");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(d => d.SupplierName!.Contains("Acme", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredDeliveries_FilterByStatus_ReturnsMatchingDeliveries()
        {
            // Arrange
            var matching1 = CreateDelivery(d => d.Status = "PENDING");
            var matching2 = CreateDelivery(d => d.Status = "PENDING");
            var nonMatching = CreateDelivery(d => d.Status = "DELIVERED");

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredDeliveries(nameof(DeliveryResponse.Status), "pending");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(d => d.Status!.Contains("PENDING", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredDeliveries_FilterByOrderDate_ReturnsDeliveriesFromThatDay()
        {
            // Arrange – two deliveries on the target date, one on a different date
            var targetDate = new DateTime(2025, 6, 15, 10, 0, 0);
            var otherDate = new DateTime(2025, 6, 16, 10, 0, 0);

            var matching1 = CreateDelivery(d => d.OrderDate = targetDate);
            var matching2 = CreateDelivery(d => d.OrderDate = targetDate.AddHours(5));
            var nonMatching = CreateDelivery(d => d.OrderDate = otherDate);

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredDeliveries(nameof(DeliveryResponse.OrderDate), "2025-06-15");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(d => d.OrderDate!.Value.Date == targetDate.Date);
        }

        [Fact]
        public async Task GetFilteredDeliveries_FilterByOrderDate_InvalidDateFormat_ReturnsEmptyList()
        {
            // Arrange – unparseable date string should yield an empty result
            var deliveries = Enumerable.Range(0, 3)
                .Select(_ => CreateDelivery())
                .ToList();

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetFilteredDeliveries(nameof(DeliveryResponse.OrderDate), "not-a-date");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFilteredDeliveries_CaseSensitiveFilter_ReturnsOnlyExactCaseMatches()
        {
            // Arrange – same supplier name, different casing
            var upperCase = CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Acme Supplies" });
            var lowerCase = CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "acme supplies" });

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { upperCase, lowerCase });

            // Act – ignoreCase: false should only match exact casing
            var result = await _sut.GetFilteredDeliveries(
                nameof(DeliveryResponse.SupplierName), "Acme Supplies", ignoreCase: false);

            // Assert
            result.Should().HaveCount(1);
            result.First().SupplierName.Should().Be("Acme Supplies");
        }

        [Fact]
        public async Task GetFilteredDeliveries_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { CreateDelivery() });

            // Act & Assert – an unsupported property name should throw ArgumentException
            await _sut.Invoking(s => s.GetFilteredDeliveries("InvalidProperty", "someFilter"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*InvalidProperty*");
        }

        #endregion

        #region GetSortedDeliveries

        [Fact]
        public async Task GetSortedDeliveries_NullPropertyName_ReturnsDeliveriesInOriginalOrder()
        {
            // Arrange – without a sort property, original repository order should be preserved
            var deliveries = Enumerable.Range(0, 3)
                .Select(_ => CreateDelivery())
                .ToList();

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetSortedDeliveries(null);

            // Assert – IDs match original order
            result.Select(r => r.DeliveryID)
                  .Should().ContainInOrder(deliveries.Select(d => d.DeliveryID));
        }

        [Fact]
        public async Task GetSortedDeliveries_BySupplierNameAscending_ReturnsSortedAscending()
        {
            // Arrange
            var deliveries = new List<Delivery>
        {
            CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Zebra Supplies" }),
            CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Acme Supplies" }),
            CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Mango Logistics" }),
        };

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetSortedDeliveries(nameof(DeliveryResponse.SupplierName), SortOrderOptions.ASC);

            // Assert
            result.Select(d => d.SupplierName)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedDeliveries_BySupplierNameDescending_ReturnsSortedDescending()
        {
            // Arrange
            var deliveries = new List<Delivery>
        {
            CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Acme Supplies" }),
            CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Zebra Supplies" }),
            CreateDelivery(d => d.Supplier = new Supplier { SupplierName = "Mango Logistics" }),
        };

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetSortedDeliveries(nameof(DeliveryResponse.SupplierName), SortOrderOptions.DESC);

            // Assert
            result.Select(d => d.SupplierName)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedDeliveries_ByOrderDateAscending_ReturnsSortedByDateAscending()
        {
            // Arrange – dates set explicitly so sort order is deterministic
            var deliveries = new List<Delivery>
        {
            CreateDelivery(d => d.OrderDate = new DateTime(2025, 3, 1)),
            CreateDelivery(d => d.OrderDate = new DateTime(2025, 1, 1)),
            CreateDelivery(d => d.OrderDate = new DateTime(2025, 2, 1)),
        };

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetSortedDeliveries(nameof(DeliveryResponse.OrderDate), SortOrderOptions.ASC);

            // Assert
            result.Select(d => d.OrderDate)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedDeliveries_ByOrderDateDescending_ReturnsSortedByDateDescending()
        {
            // Arrange
            var deliveries = new List<Delivery>
        {
            CreateDelivery(d => d.OrderDate = new DateTime(2025, 1, 1)),
            CreateDelivery(d => d.OrderDate = new DateTime(2025, 3, 1)),
            CreateDelivery(d => d.OrderDate = new DateTime(2025, 2, 1)),
        };

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetSortedDeliveries(nameof(DeliveryResponse.OrderDate), SortOrderOptions.DESC);

            // Assert
            result.Select(d => d.OrderDate)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedDeliveries_ByStatusAscending_ReturnsSortedByStatusAscending()
        {
            // Arrange
            var deliveries = new List<Delivery>
        {
            CreateDelivery(d => d.Status = "PENDING"),
            CreateDelivery(d => d.Status = "DELIVERED"),
            CreateDelivery(d => d.Status = "CANCELLED"),
        };

            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            // Act
            var result = await _sut.GetSortedDeliveries(nameof(DeliveryResponse.Status), SortOrderOptions.ASC);

            // Assert
            result.Select(d => d.Status)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedDeliveries_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _deliveriesRepositoryMock
                .Setup(r => r.GetAllDeliveries())
                .ReturnsAsync(new List<Delivery> { CreateDelivery() });

            // Act & Assert
            await _sut.Invoking(s => s.GetSortedDeliveries("NonExistentProperty"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*NonExistentProperty*");
        }

        #endregion
    }
}
