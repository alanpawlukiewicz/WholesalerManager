using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CategoryDTO;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;
using WholesalerManager.Core.Services.CategoriesServices;
using Xunit;

namespace WholesalerManager.ServiceTests.CategoryServiceTests
{
    public class CategoriesGetterServiceTests
    {
        private readonly Mock<ICategoriesRepository> _categoriesRepositoryMock;
        private readonly Mock<ILogger<CategoriesGetterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly ICategoriesGetterService _sut;

        public CategoriesGetterServiceTests()
        {
            _categoriesRepositoryMock = new Mock<ICategoriesRepository>();
            _loggerMock = new Mock<ILogger<CategoriesGetterService>>();

            _fixture = new Fixture();

            _sut = new CategoriesGetterService(
                _categoriesRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region GetAllCategories

        [Fact]
        public async Task GetAllCategories_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no categories
            _categoriesRepositoryMock
                .Setup(r => r.GetAllCategories())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _sut.GetAllCategories();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCategories_WithCategories_ReturnsAllMappedToCategoryResponse()
        {
            // Arrange – AutoFixture generates a list of random categories
            var categories = _fixture.CreateMany<Category>(3).ToList();

            _categoriesRepositoryMock
                .Setup(r => r.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _sut.GetAllCategories();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<CategoryResponse>();
            result.Select(r => r.CategoryID)
                  .Should().BeEquivalentTo(categories.Select(c => c.CategoryID));
        }

        [Fact]
        public async Task GetAllCategories_WithCategories_MapsFieldsCorrectly()
        {
            // Arrange – single category to verify field mapping precisely
            var category = _fixture.Create<Category>();

            _categoriesRepositoryMock
                .Setup(r => r.GetAllCategories())
                .ReturnsAsync(new List<Category> { category });

            // Act
            var result = await _sut.GetAllCategories();

            // Assert – all fields are correctly mapped to the response DTO
            var response = result.Single();
            response.CategoryID.Should().Be(category.CategoryID);
            response.CategoryName.Should().Be(category.CategoryName);
        }

        [Fact]
        public async Task GetAllCategories_CallsRepositoryExactlyOnce()
        {
            // Arrange
            _categoriesRepositoryMock
                .Setup(r => r.GetAllCategories())
                .ReturnsAsync(new List<Category>());

            // Act
            await _sut.GetAllCategories();

            // Assert – repository should be called exactly once
            _categoriesRepositoryMock.Verify(r => r.GetAllCategories(), Times.Once);
        }

        #endregion
    }
}
