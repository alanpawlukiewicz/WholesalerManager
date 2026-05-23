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
    public class DeliveriesUpdaterServiceTests
    {
        private readonly Mock<IDeliveriesRepository> _deliveriesRepositoryMock;
        private readonly Mock<ILogger<DeliveriesUpdaterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveriesUpdaterService _sut;

        public DeliveriesUpdaterServiceTests()
        {
            _deliveriesRepositoryMock = new Mock<IDeliveriesRepository>();
            _loggerMock = new Mock<ILogger<DeliveriesUpdaterService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveriesUpdaterService(
                _deliveriesRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid DeliveryUpdateRequest with all required fields filled
        private DeliveryUpdateRequest CreateValidUpdateRequest(Action<DeliveryUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<DeliveryUpdateRequest>()
                .With(r => r.DeliveryID, Guid.NewGuid())
                .With(r => r.SupplierID, Guid.NewGuid())
                .With(r => r.OrderDate, DateTime.UtcNow)
                .With(r => r.Status, DeliveryStatus.ORDERED)
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a Delivery entity with a Supplier navigation property
        private Delivery CreateDelivery(Action<Delivery>? configure = null)
        {
            var delivery = _fixture.Build<Delivery>()
                .With(d => d.Supplier, _fixture.Create<Supplier>())
                .Create();

            configure?.Invoke(delivery);
            return delivery;
        }

        #endregion

        #region SetDeliveryAsReceived

        [Fact]
        public async Task SetDeliveryAsReceived_EmptyGuid_ThrowsArgumentNullException()
        {
            // Act & Assert – Guid.Empty is explicitly rejected by the service
            await _sut.Invoking(s => s.SetDeliveryAsReceived(Guid.Empty))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveriesRepositoryMock.Verify(r => r.GetDeliveryById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task SetDeliveryAsReceived_NonExistentDelivery_ReturnsFalse()
        {
            // Arrange – repository returns null for the given ID
            var deliveryId = _fixture.Create<Guid>();

            _deliveriesRepositoryMock
                .Setup(r => r.GetDeliveryById(deliveryId))
                .ReturnsAsync((Delivery?)null);

            // Act
            var result = await _sut.SetDeliveryAsReceived(deliveryId);

            // Assert
            result.Should().BeFalse();
            _deliveriesRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Theory]
        [InlineData("PENDING")]
        [InlineData("RECEIVED")]
        [InlineData("CANCELLED")]
        public async Task SetDeliveryAsReceived_StatusNotInTransit_ReturnsFalse(string invalidStatus)
        {
            // Arrange – only IN_TRANSIT deliveries can be marked as received
            var deliveryId = _fixture.Create<Guid>();
            var delivery = CreateDelivery(d =>
            {
                d.DeliveryID = deliveryId;
                d.Status = invalidStatus;
            });

            _deliveriesRepositoryMock
                .Setup(r => r.GetDeliveryById(deliveryId))
                .ReturnsAsync(delivery);

            // Act
            var result = await _sut.SetDeliveryAsReceived(deliveryId);

            // Assert
            result.Should().BeFalse();
            _deliveriesRepositoryMock.Verify(r => r.Save(), Times.Never);
        }

        [Fact]
        public async Task SetDeliveryAsReceived_InTransitDelivery_ReturnsTrueAndSetsStatusToReceived()
        {
            // Arrange – only IN_TRANSIT deliveries can transition to RECEIVED
            var deliveryId = _fixture.Create<Guid>();
            var delivery = CreateDelivery(d =>
            {
                d.DeliveryID = deliveryId;
                d.Status = DeliveryStatus.IN_TRANSIT.ToString();
            });

            _deliveriesRepositoryMock
                .Setup(r => r.GetDeliveryById(deliveryId))
                .ReturnsAsync(delivery);

            _deliveriesRepositoryMock
                .Setup(r => r.Save())
                .ReturnsAsync(It.IsAny<int>);

            // Act
            var result = await _sut.SetDeliveryAsReceived(deliveryId);

            // Assert – status updated and changes persisted
            result.Should().BeTrue();
            delivery.Status.Should().Be(DeliveryStatus.RECEIVED.ToString());
            _deliveriesRepositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region UpdateDelivery

        [Fact]
        public async Task UpdateDelivery_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateDelivery(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveriesRepositoryMock.Verify(r => r.UpdateDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDelivery_MissingOrderDate_ThrowsArgumentException()
        {
            // Arrange – OrderDate is required by validation
            var request = CreateValidUpdateRequest(r => r.OrderDate = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDelivery(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveriesRepositoryMock.Verify(r => r.UpdateDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDelivery_MissingStatus_ThrowsArgumentException()
        {
            // Arrange – Status is required by validation
            var request = CreateValidUpdateRequest(r => r.Status = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDelivery(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveriesRepositoryMock.Verify(r => r.UpdateDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDelivery_RepositoryReturnsNull_ThrowsArgumentException()
        {
            // Arrange – repository returns null meaning delivery was not found
            var request = CreateValidUpdateRequest();

            _deliveriesRepositoryMock
                .Setup(r => r.UpdateDelivery(It.IsAny<Delivery>()))
                .ReturnsAsync((Delivery?)null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDelivery(request))
                      .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateDelivery_ValidRequest_ReturnsDeliveryResponse()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            var updatedDelivery = CreateDelivery(d =>
            {
                d.DeliveryID = request.DeliveryID;
                d.SupplierID = request.SupplierID;
                d.OrderDate = request.OrderDate;
                d.Status = request.Status.ToString();
            });

            _deliveriesRepositoryMock
                .Setup(r => r.UpdateDelivery(It.IsAny<Delivery>()))
                .ReturnsAsync(updatedDelivery);

            // Act
            var result = await _sut.UpdateDelivery(request);

            // Assert – response reflects the updated delivery entity
            result.Should().NotBeNull();
            result.DeliveryID.Should().Be(updatedDelivery.DeliveryID);
            result.SupplierID.Should().Be(updatedDelivery.SupplierID);
            result.OrderDate.Should().Be(updatedDelivery.OrderDate);
            result.Status.Should().Be(updatedDelivery.Status);
        }

        [Fact]
        public async Task UpdateDelivery_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            var updatedDelivery = CreateDelivery(d => d.DeliveryID = request.DeliveryID);

            _deliveriesRepositoryMock
                .Setup(r => r.UpdateDelivery(It.IsAny<Delivery>()))
                .ReturnsAsync(updatedDelivery);

            // Act
            await _sut.UpdateDelivery(request);

            // Assert
            _deliveriesRepositoryMock.Verify(r => r.UpdateDelivery(It.IsAny<Delivery>()), Times.Once);
        }

        #endregion
    }
}
