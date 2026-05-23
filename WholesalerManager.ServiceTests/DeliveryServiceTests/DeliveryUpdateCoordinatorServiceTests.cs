using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;

namespace WholesalerManager.ServiceTests.DeliveryServiceTests
{
    public class DeliveryUpdateCoordinatorServiceTests
    {
        private readonly Mock<IDeliveriesUpdaterService> _deliveriesUpdaterServiceMock;
        private readonly Mock<IDeliveryItemsUpdaterService> _deliveryItemsUpdaterServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<DeliveryUpdateCoordinatorService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveryUpdateCoordinatorService _sut;

        public DeliveryUpdateCoordinatorServiceTests()
        {
            _deliveriesUpdaterServiceMock = new Mock<IDeliveriesUpdaterService>();
            _deliveryItemsUpdaterServiceMock = new Mock<IDeliveryItemsUpdaterService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<DeliveryUpdateCoordinatorService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveryUpdateCoordinatorService(
                _deliveriesUpdaterServiceMock.Object,
                _deliveryItemsUpdaterServiceMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid DeliveryUpdateRequest with all required fields filled
        private DeliveryUpdateRequest CreateValidDeliveryUpdateRequest(Action<DeliveryUpdateRequest>? configure = null)
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

        // Creates a list of valid DeliveryItemUpdateRequests
        private List<DeliveryItemUpdateRequest> CreateValidDeliveryItemUpdateRequests(int count = 2)
        {
            return _fixture.Build<DeliveryItemUpdateRequest>()
                .With(i => i.DeliveryItemID, Guid.NewGuid())
                .With(i => i.DeliveryID, Guid.NewGuid())
                .With(i => i.ProductID, Guid.NewGuid())
                .With(i => i.PriceAtSale, "19.99")
                .CreateMany(count)
                .ToList();
        }

        // Sets up all mocks for a successful full delivery update flow
        private void SetupSuccessfulUpdate(DeliveryUpdateRequest request, DeliveryResponse deliveryResponse)
        {
            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _deliveriesUpdaterServiceMock
                .Setup(s => s.UpdateDelivery(request))
                .ReturnsAsync(deliveryResponse);

            _deliveryItemsUpdaterServiceMock
                .Setup(s => s.UpdateMultipleDeliveryItems(It.IsAny<List<DeliveryItemUpdateRequest>>()))
                .ReturnsAsync(_fixture.CreateMany<DeliveryItemResponse>().ToList());
        }

        #endregion

        #region UpdateFullDelivery

        [Fact]
        public async Task UpdateFullDelivery_NullDeliveryRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var items = CreateValidDeliveryItemUpdateRequests();

            // Act & Assert – null delivery request should throw before any service call
            await _sut.Invoking(s => s.UpdateFullDelivery(null, items))
                      .Should().ThrowAsync<ArgumentNullException>();

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _deliveriesUpdaterServiceMock.Verify(s => s.UpdateDelivery(It.IsAny<DeliveryUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFullDelivery_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            var deliveryRequest = CreateValidDeliveryUpdateRequest();

            // Act & Assert – null items list should throw before any service call
            await _sut.Invoking(s => s.UpdateFullDelivery(deliveryRequest, null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _deliveriesUpdaterServiceMock.Verify(s => s.UpdateDelivery(It.IsAny<DeliveryUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFullDelivery_ValidRequest_BeginsTransaction()
        {
            // Arrange
            var deliveryRequest = CreateValidDeliveryUpdateRequest();
            var items = CreateValidDeliveryItemUpdateRequests();
            var deliveryResponse = _fixture.Create<DeliveryResponse>();

            SetupSuccessfulUpdate(deliveryRequest, deliveryResponse);

            // Act
            await _sut.UpdateFullDelivery(deliveryRequest, items);

            // Assert – transaction must be started before any work is done
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateFullDelivery_ValidRequest_CommitsTransactionAndReturnsDeliveryResponse()
        {
            // Arrange
            var deliveryRequest = CreateValidDeliveryUpdateRequest();
            var items = CreateValidDeliveryItemUpdateRequests();
            var deliveryResponse = _fixture.Create<DeliveryResponse>();

            SetupSuccessfulUpdate(deliveryRequest, deliveryResponse);

            // Act
            var result = await _sut.UpdateFullDelivery(deliveryRequest, items);

            // Assert – transaction committed, rollback never called, correct response returned
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
            result.Should().BeEquivalentTo(deliveryResponse);
        }

        [Fact]
        public async Task UpdateFullDelivery_ValidRequest_CallsUpdateDeliveryAndUpdateMultipleDeliveryItemsOnce()
        {
            // Arrange
            var deliveryRequest = CreateValidDeliveryUpdateRequest();
            var items = CreateValidDeliveryItemUpdateRequests();
            var deliveryResponse = _fixture.Create<DeliveryResponse>();

            SetupSuccessfulUpdate(deliveryRequest, deliveryResponse);

            // Act
            await _sut.UpdateFullDelivery(deliveryRequest, items);

            // Assert – both inner services called exactly once
            _deliveriesUpdaterServiceMock.Verify(s => s.UpdateDelivery(deliveryRequest), Times.Once);
            _deliveryItemsUpdaterServiceMock.Verify(
                s => s.UpdateMultipleDeliveryItems(items), Times.Once);
        }

        [Fact]
        public async Task UpdateFullDelivery_UpdateDeliveryThrowsException_RollsBackTransactionAndRethrows()
        {
            // Arrange
            var deliveryRequest = CreateValidDeliveryUpdateRequest();
            var items = CreateValidDeliveryItemUpdateRequests();

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Simulate failure in UpdateDelivery
            _deliveriesUpdaterServiceMock
                .Setup(s => s.UpdateDelivery(deliveryRequest))
                .ThrowsAsync(new Exception("UpdateDelivery failed"));

            // Act & Assert – exception should propagate after rollback
            await _sut.Invoking(s => s.UpdateFullDelivery(deliveryRequest, items))
                      .Should().ThrowAsync<Exception>()
                      .WithMessage("UpdateDelivery failed");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateFullDelivery_UpdateMultipleDeliveryItemsThrowsException_RollsBackTransactionAndRethrows()
        {
            // Arrange
            var deliveryRequest = CreateValidDeliveryUpdateRequest();
            var items = CreateValidDeliveryItemUpdateRequests();
            var deliveryResponse = _fixture.Create<DeliveryResponse>();

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            _deliveriesUpdaterServiceMock
                .Setup(s => s.UpdateDelivery(deliveryRequest))
                .ReturnsAsync(deliveryResponse);

            // Simulate failure in UpdateMultipleDeliveryItems
            _deliveryItemsUpdaterServiceMock
                .Setup(s => s.UpdateMultipleDeliveryItems(It.IsAny<List<DeliveryItemUpdateRequest>>()))
                .ThrowsAsync(new Exception("UpdateMultipleDeliveryItems failed"));

            // Act & Assert
            await _sut.Invoking(s => s.UpdateFullDelivery(deliveryRequest, items))
                      .Should().ThrowAsync<Exception>()
                      .WithMessage("UpdateMultipleDeliveryItems failed");

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        #endregion
    }
}
