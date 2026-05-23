using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;

namespace WholesalerManager.ServiceTests.OrderServiceTests
{


    public class OrdersDeleterServiceTests
    {
        private readonly Mock<IOrdersRepository> _ordersRepositoryMock;
        private readonly Mock<ILogger<OrdersDeleterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IOrdersDeleterService _sut;

        public OrdersDeleterServiceTests()
        {
            _ordersRepositoryMock = new Mock<IOrdersRepository>();
            _loggerMock = new Mock<ILogger<OrdersDeleterService>>();
            _fixture = new Fixture();

            _sut = new OrdersDeleterService(_ordersRepositoryMock.Object, _loggerMock.Object);
        }

        #region DeleteOrderByID

        [Fact]
        public async Task DeleteOrderByID_NullId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.DeleteOrderByID(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _ordersRepositoryMock.Verify(r => r.DeleteOrderById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrderByID_EmptyGuid_ThrowsArgumentException()
        {
            // Act & Assert – Guid.Empty is explicitly rejected by the service
            await _sut.Invoking(s => s.DeleteOrderByID(Guid.Empty))
                      .Should().ThrowAsync<ArgumentException>();

            _ordersRepositoryMock.Verify(r => r.DeleteOrderById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteOrderByID_NonExistentId_ReturnsFalse()
        {
            // Arrange – repository returns false when order is not found
            var nonExistentId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.DeleteOrderById(nonExistentId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteOrderByID(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteOrderByID_ExistingId_ReturnsTrue()
        {
            // Arrange – repository confirms successful deletion
            var existingId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.DeleteOrderById(existingId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteOrderByID(existingId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteOrderByID_ValidId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var orderId = _fixture.Create<Guid>();

            _ordersRepositoryMock
                .Setup(r => r.DeleteOrderById(orderId))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteOrderByID(orderId);

            // Assert – repository should be called exactly once with the correct ID
            _ordersRepositoryMock.Verify(r => r.DeleteOrderById(orderId), Times.Once);
        }

        #endregion
    }
}
