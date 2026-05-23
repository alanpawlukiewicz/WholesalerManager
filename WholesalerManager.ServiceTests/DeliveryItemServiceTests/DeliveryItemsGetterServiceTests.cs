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
    public class DeliveryItemsGetterServiceTests
    {
        private readonly Mock<IDeliveryItemsRepository> _deliveryItemsRepositoryMock;
        private readonly Mock<ILogger<DeliveryItemsGetterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IDeliveryItemsGetterService _sut;

        public DeliveryItemsGetterServiceTests()
        {
            _deliveryItemsRepositoryMock = new Mock<IDeliveryItemsRepository>();
            _loggerMock = new Mock<ILogger<DeliveryItemsGetterService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveryItemsGetterService(
                _deliveryItemsRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a DeliveryItem with a Product navigation property to ensure ToDeliveryItemResponse() maps correctly
        private DeliveryItem CreateDeliveryItem(Action<DeliveryItem>? configure = null)
        {
            var item = _fixture.Build<DeliveryItem>()
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();

            configure?.Invoke(item);
            return item;
        }

        #endregion

        #region GetAllDeliveryItems

        [Fact]
        public async Task GetAllDeliveryItems_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no items
            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItems())
                .ReturnsAsync(new List<DeliveryItem>());

            // Act
            var result = await _sut.GetAllDeliveryItems();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllDeliveryItems_WithItems_ReturnsAllMappedToDeliveryItemResponse()
        {
            // Arrange – AutoFixture generates a list of random delivery items
            var items = Enumerable.Range(0, 3)
                .Select(_ => CreateDeliveryItem())
                .ToList();

            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItems())
                .ReturnsAsync(items);

            // Act
            var result = await _sut.GetAllDeliveryItems();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<DeliveryItemResponse>();
            result.Select(r => r.DeliveryItemID)
                  .Should().BeEquivalentTo(items.Select(i => i.DeliveryItemID));
        }

        [Fact]
        public async Task GetAllDeliveryItems_WithItems_MapsFieldsCorrectly()
        {
            // Arrange – single item to verify field mapping precisely
            var item = CreateDeliveryItem();

            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItems())
                .ReturnsAsync(new List<DeliveryItem> { item });

            // Act
            var result = await _sut.GetAllDeliveryItems();

            // Assert – all fields are correctly mapped to the response DTO
            var response = result.Single();
            response.DeliveryItemID.Should().Be(item.DeliveryItemID);
            response.DeliveryID.Should().Be(item.DeliveryID);
            response.ProductID.Should().Be(item.ProductID);
            response.Quantity.Should().Be(item.Quantity);
            response.PriceAtSale.Should().Be(item.PriceAtSale);
            response.ProductName.Should().Be(item.Product?.ProductName);
            response.SKU.Should().Be(item.Product?.SKU);
            response.ProductUnitPrice.Should().Be(item.Product?.UnitPrice);
        }

        #endregion

        #region GetAllDeliveryItemsFromDelivery

        [Fact]
        public async Task GetAllDeliveryItemsFromDelivery_NullDeliveryId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.GetAllDeliveryItemsFromDelivery(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _deliveryItemsRepositoryMock.Verify(
                r => r.GetAllDeliveryItemsFromDelivery(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAllDeliveryItemsFromDelivery_ValidDeliveryId_ReturnsEmptyListWhenNoItems()
        {
            // Arrange – delivery exists but has no items
            var deliveryId = _fixture.Create<Guid>();

            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItemsFromDelivery(deliveryId))
                .ReturnsAsync(new List<DeliveryItem>());

            // Act
            var result = await _sut.GetAllDeliveryItemsFromDelivery(deliveryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllDeliveryItemsFromDelivery_ValidDeliveryId_ReturnsAllItemsMappedToDeliveryItemResponse()
        {
            // Arrange
            var deliveryId = _fixture.Create<Guid>();
            var items = Enumerable.Range(0, 3)
                .Select(_ => CreateDeliveryItem(i => i.DeliveryID = deliveryId))
                .ToList();

            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItemsFromDelivery(deliveryId))
                .ReturnsAsync(items);

            // Act
            var result = await _sut.GetAllDeliveryItemsFromDelivery(deliveryId);

            // Assert – all items belong to the requested delivery and are correctly mapped
            result.Should().NotBeNull();
            result!.Should().HaveCount(3);
            result.Should().AllBeOfType<DeliveryItemResponse>();
            result.Should().OnlyContain(r => r.DeliveryID == deliveryId);
        }

        [Fact]
        public async Task GetAllDeliveryItemsFromDelivery_ValidDeliveryId_MapsFieldsCorrectly()
        {
            // Arrange – single item to verify field mapping precisely
            var deliveryId = _fixture.Create<Guid>();
            var item = CreateDeliveryItem(i => i.DeliveryID = deliveryId);

            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItemsFromDelivery(deliveryId))
                .ReturnsAsync(new List<DeliveryItem> { item });

            // Act
            var result = await _sut.GetAllDeliveryItemsFromDelivery(deliveryId);

            // Assert
            var response = result!.Single();
            response.DeliveryItemID.Should().Be(item.DeliveryItemID);
            response.DeliveryID.Should().Be(item.DeliveryID);
            response.ProductID.Should().Be(item.ProductID);
            response.Quantity.Should().Be(item.Quantity);
            response.PriceAtSale.Should().Be(item.PriceAtSale);
            response.ProductName.Should().Be(item.Product?.ProductName);
            response.SKU.Should().Be(item.Product?.SKU);
            response.ProductUnitPrice.Should().Be(item.Product?.UnitPrice);
        }

        [Fact]
        public async Task GetAllDeliveryItemsFromDelivery_ValidDeliveryId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var deliveryId = _fixture.Create<Guid>();

            _deliveryItemsRepositoryMock
                .Setup(r => r.GetAllDeliveryItemsFromDelivery(deliveryId))
                .ReturnsAsync(new List<DeliveryItem>());

            // Act
            await _sut.GetAllDeliveryItemsFromDelivery(deliveryId);

            // Assert – repository should be called exactly once with the correct ID
            _deliveryItemsRepositoryMock.Verify(
                r => r.GetAllDeliveryItemsFromDelivery(deliveryId), Times.Once);
        }

        #endregion
    }
}
