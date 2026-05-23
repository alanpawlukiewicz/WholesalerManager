using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;

namespace WholesalerManager.ServiceTests.DeliveryServiceTests
{
    public class DeliveriesDeleterServiceTests
    {
        private readonly Mock<IDeliveriesRepository> _deliveriesRepositoryMock;
        private readonly Mock<ILogger<DeliveriesDeleterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveriesDeleterService _sut;

        public DeliveriesDeleterServiceTests()
        {
            _deliveriesRepositoryMock = new Mock<IDeliveriesRepository>();
            _loggerMock = new Mock<ILogger<DeliveriesDeleterService>>();

            _fixture = new Fixture();

            _sut = new DeliveriesDeleterService(
                _deliveriesRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region DeleteDeliveryByID

        [Fact]
        public async Task DeleteDeliveryByID_NullId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.DeleteDeliveryByID(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveriesRepositoryMock.Verify(r => r.DeleteDeliveryById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteDeliveryByID_EmptyGuid_ThrowsArgumentException()
        {
            // Act & Assert – Guid.Empty is explicitly rejected by the service
            await _sut.Invoking(s => s.DeleteDeliveryByID(Guid.Empty))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveriesRepositoryMock.Verify(r => r.DeleteDeliveryById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteDeliveryByID_NonExistentId_ReturnsFalse()
        {
            // Arrange – repository returns false when delivery is not found
            var nonExistentId = _fixture.Create<Guid>();

            _deliveriesRepositoryMock
                .Setup(r => r.DeleteDeliveryById(nonExistentId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteDeliveryByID(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteDeliveryByID_ExistingId_ReturnsTrue()
        {
            // Arrange – repository confirms successful deletion
            var existingId = _fixture.Create<Guid>();

            _deliveriesRepositoryMock
                .Setup(r => r.DeleteDeliveryById(existingId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteDeliveryByID(existingId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteDeliveryByID_ValidId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var deliveryId = _fixture.Create<Guid>();

            _deliveriesRepositoryMock
                .Setup(r => r.DeleteDeliveryById(deliveryId))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDeliveryByID(deliveryId);

            // Assert – repository should be called exactly once with the correct ID
            _deliveriesRepositoryMock.Verify(r => r.DeleteDeliveryById(deliveryId), Times.Once);
        }

        #endregion
    }
}
