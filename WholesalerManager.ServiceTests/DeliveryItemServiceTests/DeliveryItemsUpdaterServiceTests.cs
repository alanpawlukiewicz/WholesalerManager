using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.Services.DeliveryItemServices;

namespace WholesalerManager.ServiceTests.DeliveryItemServiceTests
{
    public class DeliveryItemsUpdaterServiceTests
    {
        private readonly Mock<IDeliveryItemsRepository> _deliveryItemsRepositoryMock;
        private readonly Mock<IDeliveryItemsAdderService> _deliveryItemsAdderServiceMock;
        private readonly Mock<ILogger<DeliveryItemsUpdaterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveryItemsUpdaterService _sut;

        public DeliveryItemsUpdaterServiceTests()
        {
            _deliveryItemsRepositoryMock = new Mock<IDeliveryItemsRepository>();
            _deliveryItemsAdderServiceMock = new Mock<IDeliveryItemsAdderService>();
            _loggerMock = new Mock<ILogger<DeliveryItemsUpdaterService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveryItemsUpdaterService(
                _deliveryItemsRepositoryMock.Object,
                _deliveryItemsAdderServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid DeliveryItemUpdateRequest with a known non-empty DeliveryItemID (update path)
        private DeliveryItemUpdateRequest CreateValidUpdateRequest(Action<DeliveryItemUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<DeliveryItemUpdateRequest>()
                .With(r => r.DeliveryItemID, Guid.NewGuid())
                .With(r => r.DeliveryID, Guid.NewGuid())
                .With(r => r.ProductID, Guid.NewGuid())
                .With(r => r.Quantity, _fixture.Create<int>())
                .With(r => r.PriceAtSale, "19.99")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a DeliveryItem entity returned by the repository after updating
        private DeliveryItem CreateUpdatedDeliveryItem(DeliveryItemUpdateRequest request)
        {
            return _fixture.Build<DeliveryItem>()
                .With(i => i.DeliveryItemID, request.DeliveryItemID ?? Guid.NewGuid())
                .With(i => i.DeliveryID, request.DeliveryID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();
        }

        // Creates a DeliveryItemResponse as returned by IDeliveryItemsAdderService
        private DeliveryItemResponse CreateDeliveryItemResponse(DeliveryItemUpdateRequest request)
        {
            return _fixture.Build<DeliveryItemResponse>()
                .With(r => r.DeliveryItemID, Guid.NewGuid())
                .With(r => r.DeliveryID, request.DeliveryID)
                .With(r => r.ProductID, request.ProductID)
                .With(r => r.Quantity, request.Quantity)
                .Create();
        }

        #endregion

        #region UpdateDeliveryItem

        [Fact]
        public async Task UpdateDeliveryItem_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.UpdateDeliveryItem(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
            _deliveryItemsAdderServiceMock.Verify(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeliveryItem_MissingDeliveryID_ThrowsArgumentException()
        {
            // Arrange – DeliveryID is required by validation
            var request = CreateValidUpdateRequest(r => r.DeliveryID = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDeliveryItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
            _deliveryItemsAdderServiceMock.Verify(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeliveryItem_MissingProductID_ThrowsArgumentException()
        {
            // Arrange – ProductID is required by validation
            var request = CreateValidUpdateRequest(r => r.ProductID = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDeliveryItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
            _deliveryItemsAdderServiceMock.Verify(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()), Times.Never);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.345")]
        [InlineData("-5.00")]
        public async Task UpdateDeliveryItem_InvalidPriceAtSale_ThrowsArgumentException(string price)
        {
            // Arrange – PriceAtSale must match the money regex pattern
            var request = CreateValidUpdateRequest(r => r.PriceAtSale = price);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDeliveryItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
            _deliveryItemsAdderServiceMock.Verify(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeliveryItem_EmptyDeliveryItemID_CallsAdderServiceInsteadOfRepository()
        {
            // Arrange – DeliveryItemID == Guid.Empty signals a new item being added to an existing delivery
            var request = CreateValidUpdateRequest(r => r.DeliveryItemID = Guid.Empty);
            var addedResponse = CreateDeliveryItemResponse(request);

            _deliveryItemsAdderServiceMock
                .Setup(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()))
                .ReturnsAsync(addedResponse);

            // Act
            await _sut.UpdateDeliveryItem(request);

            // Assert – adder service called, repository update never called
            _deliveryItemsAdderServiceMock.Verify(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()), Times.Once);
            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeliveryItem_NonEmptyDeliveryItemID_CallsRepositoryInsteadOfAdderService()
        {
            // Arrange – non-empty DeliveryItemID signals an existing item to be updated
            var request = CreateValidUpdateRequest();
            var updatedItem = CreateUpdatedDeliveryItem(request);

            _deliveryItemsRepositoryMock
                .Setup(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(updatedItem);

            // Act
            await _sut.UpdateDeliveryItem(request);

            // Assert – repository update called, adder service never called
            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Once);
            _deliveryItemsAdderServiceMock.Verify(s => s.AddDeliveryItem(It.IsAny<DeliveryItemAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeliveryItem_RepositoryReturnsNull_ThrowsArgumentException()
        {
            // Arrange – repository returns null meaning item was not found
            var request = CreateValidUpdateRequest();

            _deliveryItemsRepositoryMock
                .Setup(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync((DeliveryItem?)null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateDeliveryItem(request))
                      .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateDeliveryItem_ValidRequest_MapsFieldsCorrectlyToDeliveryItemResponse()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            var updatedItem = CreateUpdatedDeliveryItem(request);

            _deliveryItemsRepositoryMock
                .Setup(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(updatedItem);

            // Act
            var result = await _sut.UpdateDeliveryItem(request);

            // Assert – response fields match the updated entity
            result.Should().NotBeNull();
            result.DeliveryItemID.Should().Be(updatedItem.DeliveryItemID);
            result.DeliveryID.Should().Be(updatedItem.DeliveryID);
            result.ProductID.Should().Be(updatedItem.ProductID);
            result.Quantity.Should().Be(updatedItem.Quantity);
            result.PriceAtSale.Should().Be(updatedItem.PriceAtSale);
        }

        #endregion

        #region UpdateMultipleDeliveryItems

        [Fact]
        public async Task UpdateMultipleDeliveryItems_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateMultipleDeliveryItems(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMultipleDeliveryItems_EmptyList_ReturnsEmptyList()
        {
            // Arrange – empty list is valid, should return empty response list
            // Act
            var result = await _sut.UpdateMultipleDeliveryItems(new List<DeliveryItemUpdateRequest?>());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMultipleDeliveryItems_ValidList_ReturnsAllUpdatedItemResponses()
        {
            // Arrange
            var requests = new List<DeliveryItemUpdateRequest>
        {
            CreateValidUpdateRequest(),
            CreateValidUpdateRequest(),
            CreateValidUpdateRequest(),
        };

            // Each request gets its own updated entity returned by the repository
            foreach (var request in requests)
            {
                var updatedItem = CreateUpdatedDeliveryItem(request);

                _deliveryItemsRepositoryMock
                    .Setup(r => r.UpdateDeliveryItem(It.Is<DeliveryItem>(i =>
                        i.DeliveryItemID == request.DeliveryItemID)))
                    .ReturnsAsync(updatedItem);
            }

            // Act
            var result = await _sut.UpdateMultipleDeliveryItems(
                requests.Cast<DeliveryItemUpdateRequest?>().ToList());

            // Assert
            result.Should().NotBeNull();
            result!.Should().HaveCount(3);
            result.Should().AllBeOfType<DeliveryItemResponse>();
        }

        [Fact]
        public async Task UpdateMultipleDeliveryItems_ValidList_CallsRepositoryOncePerItem()
        {
            // Arrange
            var requests = new List<DeliveryItemUpdateRequest>
        {
            CreateValidUpdateRequest(),
            CreateValidUpdateRequest(),
        };

            foreach (var request in requests)
            {
                var updatedItem = CreateUpdatedDeliveryItem(request);

                _deliveryItemsRepositoryMock
                    .Setup(r => r.UpdateDeliveryItem(It.Is<DeliveryItem>(i =>
                        i.DeliveryItemID == request.DeliveryItemID)))
                    .ReturnsAsync(updatedItem);
            }

            // Act
            await _sut.UpdateMultipleDeliveryItems(requests.Cast<DeliveryItemUpdateRequest?>().ToList());

            // Assert – repository called once per item in the list
            _deliveryItemsRepositoryMock.Verify(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateMultipleDeliveryItems_ListContainsNullItem_ThrowsArgumentNullException()
        {
            // Arrange – null item inside the list should trigger ArgumentNullException from UpdateDeliveryItem
            var requests = new List<DeliveryItemUpdateRequest?>
        {
            CreateValidUpdateRequest(),
            null
        };

            var updatedItem = CreateUpdatedDeliveryItem(requests[0]!);

            _deliveryItemsRepositoryMock
                .Setup(r => r.UpdateDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(updatedItem);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateMultipleDeliveryItems(requests))
                      .Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion
    }
}
