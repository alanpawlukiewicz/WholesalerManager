using AutoFixture;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.ProductServices;

namespace WholesalerManager.ServiceTests.ProductServiceTests
{


    public class ProductsDeleterServiceTests
    {
        private readonly Mock<IProductsRepository> _productsRepositoryMock;
        private readonly Mock<ILogger<ProductsDeleterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly IProductsDeleterService _sut;

        public ProductsDeleterServiceTests()
        {
            _productsRepositoryMock = new Mock<IProductsRepository>();
            _loggerMock = new Mock<ILogger<ProductsDeleterService>>();
            _fixture = new Fixture();

            _sut = new ProductsDeleterService(_productsRepositoryMock.Object, _loggerMock.Object);
        }

        #region DeleteProduct

        [Fact]
        public async Task DeleteProduct_NullId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.DeleteProduct(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsRepositoryMock.Verify(r => r.DeleteProduct(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProduct_NonExistentId_ReturnsFalse()
        {
            // Arrange – repository returns false when product is not found
            var nonExistentId = _fixture.Create<Guid>();

            _productsRepositoryMock
                .Setup(r => r.DeleteProduct(nonExistentId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteProduct(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteProduct_ExistingId_ReturnsTrue()
        {
            // Arrange – repository confirms successful deletion
            var existingId = _fixture.Create<Guid>();

            _productsRepositoryMock
                .Setup(r => r.DeleteProduct(existingId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteProduct(existingId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteProduct_ValidId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var productId = _fixture.Create<Guid>();

            _productsRepositoryMock
                .Setup(r => r.DeleteProduct(productId))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteProduct(productId);

            // Assert – repository should be called exactly once with the correct ID
            _productsRepositoryMock.Verify(r => r.DeleteProduct(productId), Times.Once);
        }

        #endregion
    }
}
