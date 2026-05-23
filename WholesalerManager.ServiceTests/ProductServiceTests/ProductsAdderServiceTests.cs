using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.ProductServices;

namespace WholesalerManager.ServiceTests.ProductServiceTests
{


    public class ProductsAdderServiceTests
    {
        private readonly Mock<IProductsRepository> _productsRepositoryMock;
        private readonly Mock<ILogger<ProductsAdderService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IProductsAdderService _sut;

        public ProductsAdderServiceTests()
        {
            _productsRepositoryMock = new Mock<IProductsRepository>();
            _loggerMock = new Mock<ILogger<ProductsAdderService>>();
            _fixture = new Fixture();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new ProductsAdderService(_productsRepositoryMock.Object, _loggerMock.Object);
        }

        #region Helpers

        // Creates a valid ProductAddRequest with all required fields filled
        private ProductAddRequest CreateValidProductAddRequest(Action<ProductAddRequest>? configure = null)
        {
            var request = _fixture.Build<ProductAddRequest>()
                .With(p => p.ProductName, _fixture.Create<string>()[..20])  // keep within MaxLength
                .With(p => p.SKU, _fixture.Create<string>()[..10])
                .With(p => p.ProductDescription, _fixture.Create<string>()[..20])
                .With(p => p.CategoryID, Guid.NewGuid())
                .With(p => p.UnitPrice, "99.99")  // valid money format
                .With(p => p.StockQuantity, _fixture.Create<int>())
                .With(p => p.ReorderLevel, _fixture.Create<int>())
                .Create();

            configure?.Invoke(request);
            return request;
        }

        #endregion

        #region AddProduct

        [Fact]
        public async Task AddProduct_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.AddProduct(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsRepositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task AddProduct_MissingProductName_ThrowsArgumentException()
        {
            // Arrange – ProductName is required, so null should fail validation
            var request = CreateValidProductAddRequest(r => r.ProductName = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task AddProduct_MissingSKU_ThrowsArgumentException()
        {
            // Arrange – SKU is required
            var request = CreateValidProductAddRequest(r => r.SKU = "");

            // Act & Assert
            await _sut.Invoking(s => s.AddProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task AddProduct_MissingCategoryID_ThrowsArgumentException()
        {
            // Arrange – CategoryID is required
            var request = CreateValidProductAddRequest(r => r.CategoryID = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]       // missing entirely
        [InlineData("")]         // empty string
        [InlineData("abc")]      // not a number
        [InlineData("12.345")]   // more than 2 decimal places
        [InlineData("-10.00")]   // negative value
        public async Task AddProduct_InvalidUnitPrice_ThrowsArgumentException(string? unitPrice)
        {
            // Arrange – UnitPrice must match the money regex pattern
            var request = CreateValidProductAddRequest(r => r.UnitPrice = unitPrice);

            // Act & Assert
            await _sut.Invoking(s => s.AddProduct(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsRepositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Never);
        }

        [Theory]
        [InlineData("0")]        // zero is valid
        [InlineData("99")]       // whole number
        [InlineData("99.9")]     // one decimal place
        [InlineData("99.99")]    // two decimal places
        [InlineData("99,99")]    // comma as decimal separator
        public async Task AddProduct_ValidUnitPriceFormats_ReturnsProductResponse(string unitPrice)
        {
            // Arrange – all these formats should pass the regex validation
            var request = CreateValidProductAddRequest(r => r.UnitPrice = unitPrice);

            _productsRepositoryMock
                .Setup(r => r.AddProduct(It.IsAny<Product>()))
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            var result = await _sut.AddProduct(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ProductResponse>();
        }

        [Fact]
        public async Task AddProduct_ValidRequest_ReturnsProductResponseWithNewGuid()
        {
            // Arrange
            var request = CreateValidProductAddRequest();

            _productsRepositoryMock
                .Setup(r => r.AddProduct(It.IsAny<Product>()))
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            var result = await _sut.AddProduct(request);

            // Assert – service should assign a new non-empty ProductID
            result.Should().NotBeNull();
            result.ProductID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddProduct_ValidRequest_MapsFieldsCorrectlyToProductResponse()
        {
            // Arrange
            var request = CreateValidProductAddRequest();

            _productsRepositoryMock
                .Setup(r => r.AddProduct(It.IsAny<Product>()))
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            var result = await _sut.AddProduct(request);

            // Assert – all fields from request are correctly reflected in the response
            result.ProductName.Should().Be(request.ProductName);
            result.SKU.Should().Be(request.SKU);
            result.ProductDescription.Should().Be(request.ProductDescription);
            result.CategoryID.Should().Be(request.CategoryID);
            result.StockQuantity.Should().Be(request.StockQuantity);
            result.ReorderLevel.Should().Be(request.ReorderLevel);
        }

        [Fact]
        public async Task AddProduct_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidProductAddRequest();

            _productsRepositoryMock
                .Setup(r => r.AddProduct(It.IsAny<Product>()))
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            await _sut.AddProduct(request);

            // Assert – repository AddProduct should be called exactly once
            _productsRepositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task AddProduct_ValidRequest_PassesProductWithCorrectFieldsToRepository()
        {
            // Arrange
            var request = CreateValidProductAddRequest();
            Product? capturedProduct = null;

            // Capture the product passed to the repository to verify its contents
            _productsRepositoryMock
                .Setup(r => r.AddProduct(It.IsAny<Product>()))
                .Callback<Product>(p => capturedProduct = p)
                .ReturnsAsync(It.IsAny<Product>());

            // Act
            await _sut.AddProduct(request);

            // Assert – the product passed to the repository should match the request
            capturedProduct.Should().NotBeNull();
            capturedProduct!.ProductName.Should().Be(request.ProductName);
            capturedProduct.SKU.Should().Be(request.SKU);
            capturedProduct.CategoryID.Should().Be(request.CategoryID);
            capturedProduct.ProductID.Should().NotBeEmpty();
        }

        #endregion
    }
}
