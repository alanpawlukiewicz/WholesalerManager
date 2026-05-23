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
    public class DeliveryItemsAdderServiceTests
    {
        private readonly Mock<IDeliveryItemsRepository> _deliveryItemsRepositoryMock;
        private readonly Mock<ILogger<DeliveryItemsAdderService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveryItemsAdderService _sut;

        public DeliveryItemsAdderServiceTests()
        {
            _deliveryItemsRepositoryMock = new Mock<IDeliveryItemsRepository>();
            _loggerMock = new Mock<ILogger<DeliveryItemsAdderService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveryItemsAdderService(
                _deliveryItemsRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid DeliveryItemAddRequest with all required fields filled
        private DeliveryItemAddRequest CreateValidItemAddRequest(Action<DeliveryItemAddRequest>? configure = null)
        {
            var request = _fixture.Build<DeliveryItemAddRequest>()
                .With(r => r.DeliveryID, Guid.NewGuid())
                .With(r => r.ProductID, Guid.NewGuid())
                .With(r => r.Quantity, _fixture.Create<int>())
                .With(r => r.PriceAtSale, "19.99")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a DeliveryItem entity returned by the repository after adding
        private DeliveryItem CreateAddedDeliveryItem(DeliveryItemAddRequest request)
        {
            return _fixture.Build<DeliveryItem>()
                .With(i => i.DeliveryID, request.DeliveryID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();
        }

        #endregion

        #region AddDeliveryItem

        [Fact]
        public async Task AddDeliveryItem_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.AddDeliveryItem(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Fact]
        public async Task AddDeliveryItem_MissingProductID_ThrowsArgumentException()
        {
            // Arrange – ProductID is required by validation
            var request = CreateValidItemAddRequest(r => r.ProductID = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddDeliveryItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.345")]
        [InlineData("-5.00")]
        public async Task AddDeliveryItem_InvalidPriceAtSale_ThrowsArgumentException(string price)
        {
            // Arrange – PriceAtSale must match the money regex pattern
            var request = CreateValidItemAddRequest(r => r.PriceAtSale = price);

            // Act & Assert
            await _sut.Invoking(s => s.AddDeliveryItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AddDeliveryItem_NullOrEmptyPriceAtSale_DoesNotThrowAndSetsPriceToZero(string? price)
        {
            // Arrange – null and empty string pass validation (no [Required] attribute)
            // and ToDecimalSafe() converts them to 0
            var request = CreateValidItemAddRequest(r => r.PriceAtSale = price);
            var addedItem = _fixture.Build<DeliveryItem>()
                .With(i => i.DeliveryID, request.DeliveryID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.PriceAtSale, 0m)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();

            _deliveryItemsRepositoryMock
                .Setup(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.Invoking(s => s.AddDeliveryItem(request))
                                    .Should().NotThrowAsync();

            // Assert – price defaults to 0 when null or empty is provided
            result.Subject.PriceAtSale.Should().Be(0m);
        }

        [Fact]
        public async Task AddDeliveryItem_ValidRequest_ReturnsDeliveryItemResponseWithNewGuid()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedDeliveryItem(request);

            _deliveryItemsRepositoryMock
                .Setup(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.AddDeliveryItem(request);

            // Assert – service should assign a new non-empty DeliveryItemID
            result.Should().NotBeNull();
            result.DeliveryItemID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddDeliveryItem_ValidRequest_MapsFieldsCorrectlyToDeliveryItemResponse()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedDeliveryItem(request);

            _deliveryItemsRepositoryMock
                .Setup(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.AddDeliveryItem(request);

            // Assert – all fields from the added entity are reflected in the response
            result.DeliveryID.Should().Be(addedItem.DeliveryID);
            result.ProductID.Should().Be(addedItem.ProductID);
            result.Quantity.Should().Be(addedItem.Quantity);
            result.PriceAtSale.Should().Be(addedItem.PriceAtSale);
            result.ProductName.Should().Be(addedItem.Product?.ProductName);
            result.SKU.Should().Be(addedItem.Product?.SKU);
            result.ProductUnitPrice.Should().Be(addedItem.Product?.UnitPrice);
        }

        [Fact]
        public async Task AddDeliveryItem_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedDeliveryItem(request);

            _deliveryItemsRepositoryMock
                .Setup(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(addedItem);

            // Act
            await _sut.AddDeliveryItem(request);

            // Assert
            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Once);
        }

        [Fact]
        public async Task AddDeliveryItem_ValidRequest_PassesItemWithNewGuidToRepository()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedDeliveryItem(request);
            DeliveryItem? capturedItem = null;

            // Capture the item passed to the repository to verify its contents
            _deliveryItemsRepositoryMock
                .Setup(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()))
                .Callback<DeliveryItem>(i => capturedItem = i)
                .ReturnsAsync(addedItem);

            // Act
            await _sut.AddDeliveryItem(request);

            // Assert – the item passed to the repository should have a new non-empty ID
            capturedItem.Should().NotBeNull();
            capturedItem!.DeliveryItemID.Should().NotBeEmpty();
            capturedItem.DeliveryID.Should().Be(request.DeliveryID);
            capturedItem.ProductID.Should().Be(request.ProductID);
            capturedItem.Quantity.Should().Be(request.Quantity);
        }

        #endregion

        #region AddMultipleDeliveryItems

        [Fact]
        public async Task AddMultipleDeliveryItems_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert – null list should throw before any repository call
            await _sut.Invoking(s => s.AddMultipleDeliveryItems(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Fact]
        public async Task AddMultipleDeliveryItems_EmptyList_ReturnsEmptyList()
        {
            // Arrange – empty list is valid, should return empty response list
            // Act
            var result = await _sut.AddMultipleDeliveryItems(new List<DeliveryItemAddRequest>());

            // Assert
            result.Should().BeEmpty();
            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Never);
        }

        [Fact]
        public async Task AddMultipleDeliveryItems_ValidList_ReturnsAllAddedItemResponses()
        {
            // Arrange
            var requests = new List<DeliveryItemAddRequest>
        {
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
        };

            // Each request gets its own added entity returned by the repository
            foreach (var request in requests)
            {
                var addedItem = CreateAddedDeliveryItem(request);

                _deliveryItemsRepositoryMock
                    .Setup(r => r.AddDeliveryItem(It.Is<DeliveryItem>(i =>
                        i.DeliveryID == request.DeliveryID &&
                        i.ProductID == request.ProductID)))
                    .ReturnsAsync(addedItem);
            }

            // Act
            var result = await _sut.AddMultipleDeliveryItems(requests);

            // Assert – all items processed and returned
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<DeliveryItemResponse>();
        }

        [Fact]
        public async Task AddMultipleDeliveryItems_ValidList_CallsRepositoryOncePerItem()
        {
            // Arrange
            var requests = new List<DeliveryItemAddRequest>
        {
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
        };

            foreach (var request in requests)
            {
                var addedItem = CreateAddedDeliveryItem(request);

                _deliveryItemsRepositoryMock
                    .Setup(r => r.AddDeliveryItem(It.Is<DeliveryItem>(i =>
                        i.DeliveryID == request.DeliveryID &&
                        i.ProductID == request.ProductID)))
                    .ReturnsAsync(addedItem);
            }

            // Act
            await _sut.AddMultipleDeliveryItems(requests);

            // Assert – repository called once per item in the list
            _deliveryItemsRepositoryMock.Verify(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AddMultipleDeliveryItems_ListWithInvalidItem_ThrowsArgumentNullException()
        {
            // Arrange – null item inside the list should trigger ArgumentNullException from AddDeliveryItem
            var requests = new List<DeliveryItemAddRequest?>
        {
            CreateValidItemAddRequest(),
            null
        };

            var addedItem = CreateAddedDeliveryItem(requests[0]!);

            _deliveryItemsRepositoryMock
                .Setup(r => r.AddDeliveryItem(It.IsAny<DeliveryItem>()))
                .ReturnsAsync(addedItem);

            // Act & Assert
            await _sut.Invoking(s => s.AddMultipleDeliveryItems(requests!))
                      .Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion
    }
}
