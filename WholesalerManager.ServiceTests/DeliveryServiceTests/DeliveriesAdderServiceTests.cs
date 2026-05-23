using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.SupplierDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;

namespace WholesalerManager.ServiceTests.DeliveryServiceTests
{
    public class DeliveriesAdderServiceTests
    {
        private readonly Mock<IDeliveriesRepository> _deliveriesRepositoryMock;
        private readonly Mock<ISuppliersGetterService> _suppliersGetterServiceMock;
        private readonly Mock<ILogger<DeliveriesAdderService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveriesAdderService _sut;

        public DeliveriesAdderServiceTests()
        {
            _deliveriesRepositoryMock = new Mock<IDeliveriesRepository>();
            _suppliersGetterServiceMock = new Mock<ISuppliersGetterService>();
            _loggerMock = new Mock<ILogger<DeliveriesAdderService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveriesAdderService(
                _deliveriesRepositoryMock.Object,
                _suppliersGetterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid DeliveryAddRequest with all required fields filled
        private DeliveryAddRequest CreateValidAddRequest(Action<DeliveryAddRequest>? configure = null)
        {
            var request = _fixture.Build<DeliveryAddRequest>()
                .With(r => r.SupplierID, Guid.NewGuid())
                .With(r => r.OrderDate, DateTime.UtcNow)
                .With(r => r.Status, DeliveryStatus.ORDERED)
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a Delivery entity returned by the repository after adding
        private Delivery CreateAddedDelivery(DeliveryAddRequest request)
        {
            return _fixture.Build<Delivery>()
                .With(d => d.SupplierID, request.SupplierID)
                .With(d => d.OrderDate, request.OrderDate)
                .With(d => d.Status, request.Status.ToString())
                .With(d => d.Supplier, _fixture.Create<Supplier>())
                .Create();
        }

        #endregion

        #region AddDelivery

        [Fact]
        public async Task AddDelivery_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any service or repository call
            await _sut.Invoking(s => s.AddDelivery(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _suppliersGetterServiceMock.Verify(s => s.GetSupplierByID(It.IsAny<Guid?>()), Times.Never);
            _deliveriesRepositoryMock.Verify(r => r.AddDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task AddDelivery_MissingSupplierID_ThrowsArgumentException()
        {
            // Arrange – SupplierID is required by validation
            var request = CreateValidAddRequest(r => r.SupplierID = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddDelivery(request))
                      .Should().ThrowAsync<ArgumentException>();

            _suppliersGetterServiceMock.Verify(s => s.GetSupplierByID(It.IsAny<Guid?>()), Times.Never);
            _deliveriesRepositoryMock.Verify(r => r.AddDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task AddDelivery_MissingOrderDate_ThrowsArgumentException()
        {
            // Arrange – OrderDate is required by validation
            var request = CreateValidAddRequest(r => r.OrderDate = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddDelivery(request))
                      .Should().ThrowAsync<ArgumentException>();

            _suppliersGetterServiceMock.Verify(s => s.GetSupplierByID(It.IsAny<Guid?>()), Times.Never);
            _deliveriesRepositoryMock.Verify(r => r.AddDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task AddDelivery_NonExistentSupplier_ThrowsArgumentException()
        {
            // Arrange – supplier lookup returns null, meaning supplier does not exist
            var request = CreateValidAddRequest();

            _suppliersGetterServiceMock
                .Setup(s => s.GetSupplierByID(request.SupplierID))
                .ReturnsAsync((SupplierResponse?)null);

            // Act & Assert
            await _sut.Invoking(s => s.AddDelivery(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveriesRepositoryMock.Verify(r => r.AddDelivery(It.IsAny<Delivery>()), Times.Never);
        }

        [Fact]
        public async Task AddDelivery_ValidRequest_ReturnsDeliveryResponseWithNewGuid()
        {
            // Arrange
            var request = CreateValidAddRequest();
            var supplierResponse = _fixture.Create<SupplierResponse>();
            var addedDelivery = CreateAddedDelivery(request);

            _suppliersGetterServiceMock
                .Setup(s => s.GetSupplierByID(request.SupplierID))
                .ReturnsAsync(supplierResponse);

            _deliveriesRepositoryMock
                .Setup(r => r.AddDelivery(It.IsAny<Delivery>()))
                .ReturnsAsync(addedDelivery);

            // Act
            var result = await _sut.AddDelivery(request);

            // Assert – service should assign a new non-empty DeliveryID
            result.Should().NotBeNull();
            result.DeliveryID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddDelivery_ValidRequest_MapsFieldsCorrectlyToDeliveryResponse()
        {
            // Arrange
            var request = CreateValidAddRequest();
            var supplierResponse = _fixture.Create<SupplierResponse>();
            var addedDelivery = CreateAddedDelivery(request);

            _suppliersGetterServiceMock
                .Setup(s => s.GetSupplierByID(request.SupplierID))
                .ReturnsAsync(supplierResponse);

            _deliveriesRepositoryMock
                .Setup(r => r.AddDelivery(It.IsAny<Delivery>()))
                .ReturnsAsync(addedDelivery);

            // Act
            var result = await _sut.AddDelivery(request);

            // Assert – response fields match the added delivery entity
            result.SupplierID.Should().Be(addedDelivery.SupplierID);
            result.OrderDate.Should().Be(addedDelivery.OrderDate);
            result.Status.Should().Be(addedDelivery.Status);
            result.SupplierName.Should().Be(addedDelivery.Supplier?.SupplierName);
        }

        [Fact]
        public async Task AddDelivery_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidAddRequest();
            var supplierResponse = _fixture.Create<SupplierResponse>();
            var addedDelivery = CreateAddedDelivery(request);

            _suppliersGetterServiceMock
                .Setup(s => s.GetSupplierByID(request.SupplierID))
                .ReturnsAsync(supplierResponse);

            _deliveriesRepositoryMock
                .Setup(r => r.AddDelivery(It.IsAny<Delivery>()))
                .ReturnsAsync(addedDelivery);

            // Act
            await _sut.AddDelivery(request);

            // Assert
            _deliveriesRepositoryMock.Verify(r => r.AddDelivery(It.IsAny<Delivery>()), Times.Once);
        }

        [Fact]
        public async Task AddDelivery_ValidRequest_PassesDeliveryWithNewGuidToRepository()
        {
            // Arrange
            var request = CreateValidAddRequest();
            var supplierResponse = _fixture.Create<SupplierResponse>();
            var addedDelivery = CreateAddedDelivery(request);
            Delivery? capturedDelivery = null;

            _suppliersGetterServiceMock
                .Setup(s => s.GetSupplierByID(request.SupplierID))
                .ReturnsAsync(supplierResponse);

            // Capture the delivery passed to the repository to verify its contents
            _deliveriesRepositoryMock
                .Setup(r => r.AddDelivery(It.IsAny<Delivery>()))
                .Callback<Delivery>(d => capturedDelivery = d)
                .ReturnsAsync(addedDelivery);

            // Act
            await _sut.AddDelivery(request);

            // Assert – the delivery passed to the repository should have a new non-empty ID
            capturedDelivery.Should().NotBeNull();
            capturedDelivery!.DeliveryID.Should().NotBeEmpty();
            capturedDelivery.SupplierID.Should().Be(request.SupplierID);
            capturedDelivery.OrderDate.Should().Be(request.OrderDate);
            capturedDelivery.Status.Should().Be(request.Status.ToString());
        }

        #endregion
    }
}
