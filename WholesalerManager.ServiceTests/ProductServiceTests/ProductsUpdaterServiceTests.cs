using AutoFixture;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.ProductServices;
using Xunit;

namespace WholesalerManager.ServiceTests.ProductServiceTests
{
    

    public class ProductsUpdaterServiceTests
    {
        private readonly Mock<IProductsRepository> _productsRepositoryMock;
        private readonly IFixture _fixture;
        private readonly IProductsUpdaterService _sut;
        private readonly Mock<ILogger<ProductsUpdaterService>> _loggerMock;

        public ProductsUpdaterServiceTests()
        {
            _productsRepositoryMock = new Mock<IProductsRepository>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _loggerMock = new Mock<ILogger<ProductsUpdaterService>>();

            _sut = new ProductsUpdaterService(_productsRepositoryMock.Object, _loggerMock.Object);
        }

        #region Helpers

        // Creates a valid ProductUpdateRequest with all required fields filled
        private ProductUpdateRequest CreateValidUpdateRequest(Action<ProductUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<ProductUpdateRequest>()
                .With(r => r.ProductID, Guid.NewGuid())
                .With(r => r.ProductName, _fixture.Create<string>()[..20])
                .With(r => r.SKU, _fixture.Create<string>()[..10])
                .With(r => r.ProductDescription, _fixture.Create<string>()[..20])
                .With(r => r.CategoryID, Guid.NewGuid())
                .With(r => r.UnitPrice, "99.99")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a Product entity matching the given update request
        private Product CreateMatchingProduct(ProductUpdateRequest request)
        {
            return _fixture.Build<Product>()
                .With(p => p.ProductID, request.ProductID)
                .With(p => p.Category, _fixture.Create<Category>())
                .Create();
        }

        #endregion

        #region UpdateProduct

        [Fact]
        public async Task UpdateProduct_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.UpdateProduct(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
            _productsRepositoryMock.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_MissingProductName_ThrowsArgumentException()
        {
            // Arrange – ProductName is required
            var request = CreateValidUpdateRequest(r => r.ProductName = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_MissingCategoryID_ThrowsArgumentException()
        {
            // Arrange – CategoryID is required
            var request = CreateValidUpdateRequest(r => r.CategoryID = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("12.345")]
        [InlineData("-10.00")]
        public async Task UpdateProduct_InvalidUnitPrice_ThrowsArgumentException(string? unitPrice)
        {
            // Arrange – UnitPrice must match the money regex pattern
            var request = CreateValidUpdateRequest(r => r.UnitPrice = unitPrice);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_NonExistentProductId_ThrowsArgumentException()
        {
            // Arrange – repository returns null, meaning product does not exist
            var request = CreateValidUpdateRequest();

            _productsRepositoryMock
                .Setup(r => r.GetProductById(request.ProductID))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_ValidRequest_ReturnsUpdatedProductResponse()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            var matchingProduct = CreateMatchingProduct(request);

            _productsRepositoryMock
                .Setup(r => r.GetProductById(request.ProductID))
                .ReturnsAsync(matchingProduct);

            _productsRepositoryMock
                .Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            var result = await _sut.UpdateProduct(request);

            // Assert – response reflects the updated values from the request
            result.Should().NotBeNull();
            result.ProductID.Should().Be(request.ProductID);
            result.ProductName.Should().Be(request.ProductName);
            result.SKU.Should().Be(request.SKU);
            result.CategoryID.Should().Be(request.CategoryID);
        }

        [Fact]
        public async Task UpdateProduct_ValidRequest_CallsRepositoryUpdateExactlyOnce()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            var matchingProduct = CreateMatchingProduct(request);

            _productsRepositoryMock
                .Setup(r => r.GetProductById(request.ProductID))
                .ReturnsAsync(matchingProduct);

            _productsRepositoryMock
                .Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            await _sut.UpdateProduct(request);

            // Assert
            _productsRepositoryMock.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Once);
        }

        #endregion

        #region UpdateStockQuantity

        [Fact]
        public async Task UpdateStockQuantity_NullDTO_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateStockQuantity(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStockQuantity_EmptyProductID_ReturnsFalse()
        {
            // Arrange – Guid.Empty is treated as invalid
            var dto = new EditStockQuantityDTO
            {
                ProductID = Guid.Empty,
                NewStockQuantity = 10
            };

            // Act
            var result = await _sut.UpdateStockQuantity(dto);

            // Assert
            result.Should().BeFalse();
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStockQuantity_NegativeStockQuantity_ReturnsFalse()
        {
            // Arrange – negative stock quantity is invalid
            var dto = new EditStockQuantityDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewStockQuantity = -1
            };

            // Act
            var result = await _sut.UpdateStockQuantity(dto);

            // Assert
            result.Should().BeFalse();
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStockQuantity_NonExistentProduct_ReturnsFalse()
        {
            // Arrange – repository returns null for given ID
            var dto = new EditStockQuantityDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewStockQuantity = 10
            };

            _productsRepositoryMock
                .Setup(r => r.GetProductById(dto.ProductID))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _sut.UpdateStockQuantity(dto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateStockQuantity_ValidDTO_ReturnsTrueAndSavesChanges()
        {
            // Arrange
            var product = _fixture.Build<Product>()
                .With(p => p.Category, _fixture.Create<Category>())
                .Create();

            var dto = new EditStockQuantityDTO
            {
                ProductID = product.ProductID,
                NewStockQuantity = 50
            };

            _productsRepositoryMock
                .Setup(r => r.GetProductById(dto.ProductID))
                .ReturnsAsync(product);

            _productsRepositoryMock
                .Setup(r => r.Save())
                .ReturnsAsync(It.IsAny<int>);

            // Act
            var result = await _sut.UpdateStockQuantity(dto);

            // Assert – should return true and persist changes
            result.Should().BeTrue();
            product.StockQuantity.Should().Be(50);
            _productsRepositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion

        #region UpdateUnitPrice

        [Fact]
        public async Task UpdateUnitPrice_NullDTO_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateUnitPrice(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUnitPrice_EmptyProductID_ReturnsFalse()
        {
            // Arrange – Guid.Empty is treated as invalid
            var dto = new EditUnitPriceDTO
            {
                ProductID = Guid.Empty,
                NewUnitPrice = "99.99"
            };

            // Act
            var result = await _sut.UpdateUnitPrice(dto);

            // Assert
            result.Should().BeFalse();
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-5.00")]
        public async Task UpdateUnitPrice_InvalidOrZeroPrice_ReturnsFalse(string? newUnitPrice)
        {
            // Arrange – price must be a positive decimal value
            var dto = new EditUnitPriceDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewUnitPrice = newUnitPrice
            };

            // Act
            var result = await _sut.UpdateUnitPrice(dto);

            // Assert
            result.Should().BeFalse();
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUnitPrice_NonExistentProduct_ReturnsFalse()
        {
            // Arrange – repository returns null for given ID
            var dto = new EditUnitPriceDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewUnitPrice = "49.99"
            };

            _productsRepositoryMock
                .Setup(r => r.GetProductById(dto.ProductID))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _sut.UpdateUnitPrice(dto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUnitPrice_ValidDTO_ReturnsTrueAndSavesChanges()
        {
            // Arrange
            var product = _fixture.Build<Product>()
                .With(p => p.Category, _fixture.Create<Category>())
                .Create();

            var dto = new EditUnitPriceDTO
            {
                ProductID = product.ProductID,
                NewUnitPrice = "149.99"
            };

            _productsRepositoryMock
                .Setup(r => r.GetProductById(dto.ProductID))
                .ReturnsAsync(product);

            _productsRepositoryMock
                .Setup(r => r.Save())
                .ReturnsAsync(It.IsAny<int>);

            // Act
            var result = await _sut.UpdateUnitPrice(dto);

            // Assert – should return true and persist the new price
            result.Should().BeTrue();
            product.UnitPrice.Should().Be(149.99m);
            _productsRepositoryMock.Verify(r => r.Save(), Times.Once);
        }

        #endregion
    }
}
