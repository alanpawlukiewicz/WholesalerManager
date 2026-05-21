using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.ProductServices;
using Xunit;

namespace WholesalerManager.ServiceTests
{
    public class ProductsGetterServiceTests
    {
        private readonly Mock<IProductsRepository> _productsRepositoryMock;
        private readonly Mock<ILogger<ProductsGetterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IProductsGetterService _sut;

        public ProductsGetterServiceTests()
        {
            _productsRepositoryMock = new Mock<IProductsRepository>();
            _loggerMock = new Mock<ILogger<ProductsGetterService>>();

            _fixture = new Fixture();

            // Avoid circular reference between Product and Category
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new ProductsGetterService(
                _productsRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a Product with a valid Category; optional overrides via configure action
        private Product CreateProduct(Action<Product>? configure = null)
        {
            var product = _fixture.Build<Product>()
                .With(p => p.Category, _fixture.Create<Category>())
                .Create();

            configure?.Invoke(product);
            return product;
        }

        #endregion

        #region GetAllProducts

        [Fact]
        public async Task GetAllProducts_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no products
            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _sut.GetAllProducts();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllProducts_WithProducts_ReturnsAllMappedToProductResponse()
        {
            // Arrange – AutoFixture generates a list of random products
            var products = _fixture.Build<Product>()
                .With(p => p.Category, _fixture.Create<Category>())
                .CreateMany(3)
                .ToList();

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetAllProducts();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<ProductResponse>();
            result.Select(r => r.ProductID)
                  .Should().BeEquivalentTo(products.Select(p => p.ProductID));
        }

        #endregion

        #region GetProductById

        [Fact]
        public async Task GetProductById_NullId_ReturnsNull()
        {
            // Act
            var result = await _sut.GetProductById(null);

            // Assert – repository should never be called when ID is null
            result.Should().BeNull();
            _productsRepositoryMock.Verify(r => r.GetProductById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetProductById_NonExistentId_ReturnsNull()
        {
            // Arrange – repository returns null for a random unknown ID
            var nonExistentId = _fixture.Create<Guid>();

            _productsRepositoryMock
                .Setup(r => r.GetProductById(nonExistentId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _sut.GetProductById(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductById_ValidId_ReturnsMatchingProductResponse()
        {
            // Arrange – AutoFixture creates a product with a known ID
            var product = CreateProduct();

            _productsRepositoryMock
                .Setup(r => r.GetProductById(product.ProductID))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.GetProductById(product.ProductID);

            // Assert – returned DTO matches the source entity
            result.Should().NotBeNull();
            result!.ProductID.Should().Be(product.ProductID);
            result.ProductName.Should().Be(product.ProductName);
            result.SKU.Should().Be(product.SKU);
        }

        #endregion

        #region GetFilteredProducts

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("ProductName", null)]
        [InlineData("ProductName", "")]
        [InlineData(null, "filter")]
        public async Task GetFilteredProducts_NullOrEmptyFilterOrProperty_ReturnsAllProducts(
            string? propertyName, string? filter)
        {
            // Arrange – when filter or property name is empty, all products should be returned
            var products = _fixture.Build<Product>()
                .With(p => p.Category, _fixture.Create<Category>())
                .CreateMany(3)
                .ToList();

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetFilteredProducts(propertyName, filter);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetFilteredProducts_FilterByProductName_ReturnsMatchingProducts()
        {
            // Arrange – two products share a common name fragment, one does not
            var matchingProduct1 = CreateProduct(p => p.ProductName = "Gaming Laptop");
            var matchingProduct2 = CreateProduct(p => p.ProductName = "Office Laptop");
            var nonMatchingProduct = CreateProduct(p => p.ProductName = "Wireless Mouse");

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product> { matchingProduct1, matchingProduct2, nonMatchingProduct });

            // Act – filter by "Laptop" (case-insensitive by default)
            var result = await _sut.GetFilteredProducts(nameof(ProductResponse.ProductName), "laptop");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.ProductName!.Contains("Laptop", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredProducts_FilterBySKU_ReturnsMatchingProducts()
        {
            // Arrange
            var matchingProduct1 = CreateProduct(p => p.SKU = "SKU-LAP-001");
            var matchingProduct2 = CreateProduct(p => p.SKU = "SKU-LAP-002");
            var nonMatchingProduct = CreateProduct(p => p.SKU = "SKU-MOU-001");

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product> { matchingProduct1, matchingProduct2, nonMatchingProduct });

            // Act
            var result = await _sut.GetFilteredProducts(nameof(ProductResponse.SKU), "SKU-LAP");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.SKU.Contains("SKU-LAP", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredProducts_FilterByCategoryName_ReturnsMatchingProducts()
        {
            // Arrange – two products in "Electronics", one in "Furniture"
            var electronics1 = CreateProduct(p => p.Category = new Category { CategoryName = "Electronics" });
            var electronics2 = CreateProduct(p => p.Category = new Category { CategoryName = "Electronics" });
            var furniture = CreateProduct(p => p.Category = new Category { CategoryName = "Furniture" });

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product> { electronics1, electronics2, furniture });

            // Act
            var result = await _sut.GetFilteredProducts(nameof(ProductResponse.CategoryName), "electronics");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.CategoryName!.Contains("Electronics", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredProducts_CaseSensitiveFilter_ReturnsOnlyExactCaseMatches()
        {
            // Arrange – same name, different casing
            var upperCase = CreateProduct(p => p.ProductName = "Laptop");
            var lowerCase = CreateProduct(p => p.ProductName = "laptop");

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product> { upperCase, lowerCase });

            // Act – ignoreCase: false should only match exact casing
            var result = await _sut.GetFilteredProducts(
                nameof(ProductResponse.ProductName), "Laptop", ignoreCase: false);

            // Assert
            result.Should().HaveCount(1);
            result.First().ProductName.Should().Be("Laptop");
        }

        [Fact]
        public async Task GetFilteredProducts_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product> { CreateProduct() });

            // Act & Assert – an unsupported property name should throw ArgumentException
            await _sut.Invoking(s => s.GetFilteredProducts("InvalidProperty", "someFilter"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*InvalidProperty*");
        }

        #endregion

        #region GetSortedProducts

        [Fact]
        public async Task GetSortedProducts_NullPropertyName_ReturnsProductsInOriginalOrder()
        {
            // Arrange – without a sort property, original repository order should be preserved
            var products = _fixture.Build<Product>()
                .With(p => p.Category, _fixture.Create<Category>())
                .CreateMany(3)
                .ToList();

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetSortedProducts(null);

            // Assert – IDs match original order
            result.Select(r => r.ProductID)
                  .Should().ContainInOrder(products.Select(p => p.ProductID));
        }

        [Fact]
        public async Task GetSortedProducts_ByProductNameAscending_ReturnsSortedAscending()
        {
            // Arrange
            var products = new List<Product>
        {
            CreateProduct(p => p.ProductName = "Zebra"),
            CreateProduct(p => p.ProductName = "Apple"),
            CreateProduct(p => p.ProductName = "Mango"),
        };

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetSortedProducts(nameof(ProductResponse.ProductName), SortOrderOptions.ASC);

            // Assert
            result.Select(p => p.ProductName)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedProducts_ByProductNameDescending_ReturnsSortedDescending()
        {
            // Arrange
            var products = new List<Product>
        {
            CreateProduct(p => p.ProductName = "Apple"),
            CreateProduct(p => p.ProductName = "Zebra"),
            CreateProduct(p => p.ProductName = "Mango"),
        };

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetSortedProducts(nameof(ProductResponse.ProductName), SortOrderOptions.DESC);

            // Assert
            result.Select(p => p.ProductName)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedProducts_ByUnitPriceAscending_ReturnsSortedByPriceAscending()
        {
            // Arrange – prices set explicitly so sort order is deterministic
            var products = new List<Product>
        {
            CreateProduct(p => p.UnitPrice = 299.99m),
            CreateProduct(p => p.UnitPrice = 49.99m),
            CreateProduct(p => p.UnitPrice = 149.99m),
        };

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetSortedProducts(nameof(ProductResponse.UnitPrice), SortOrderOptions.ASC);

            // Assert
            result.Select(p => p.UnitPrice)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedProducts_ByStockQuantityDescending_ReturnsSortedByStockDescending()
        {
            // Arrange
            var products = new List<Product>
        {
            CreateProduct(p => p.StockQuantity = 5),
            CreateProduct(p => p.StockQuantity = 100),
            CreateProduct(p => p.StockQuantity = 50),
        };

            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetSortedProducts(nameof(ProductResponse.StockQuantity), SortOrderOptions.DESC);

            // Assert
            result.Select(p => p.StockQuantity)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedProducts_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _productsRepositoryMock
                .Setup(r => r.GetAllProducts())
                .ReturnsAsync(new List<Product> { CreateProduct() });

            // Act & Assert
            await _sut.Invoking(s => s.GetSortedProducts("NonExistentProperty"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*NonExistentProperty*");
        }

        #endregion
    }
}
