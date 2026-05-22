using AutoFixture;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.SupplierDTO;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;
using WholesalerManager.Core.Services.SupplierServices;
using Xunit;

namespace WholesalerManager.ServiceTests
{
    

    public class SuppliersGetterServiceTests
    {
        private readonly Mock<ISuppliersRepository> _suppliersRepositoryMock;
        private readonly IFixture _fixture;
        private readonly ISuppliersGetterService _sut;
        private readonly Mock<ILogger<SuppliersGetterService>> _loggerMock;

        public SuppliersGetterServiceTests()
        {
            _suppliersRepositoryMock = new Mock<ISuppliersRepository>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _loggerMock = new Mock<ILogger<SuppliersGetterService>>();

            _sut = new SuppliersGetterService(_suppliersRepositoryMock.Object, _loggerMock.Object);
        }

        #region GetAllSuppliers

        [Fact]
        public async Task GetAllSuppliers_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no suppliers
            _suppliersRepositoryMock
                .Setup(r => r.GetAllSuppliers())
                .ReturnsAsync(new List<Supplier>());

            // Act
            var result = await _sut.GetAllSuppliers();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllSuppliers_WithSuppliers_ReturnsAllMappedToSupplierResponse()
        {
            // Arrange – AutoFixture generates a list of random suppliers
            var suppliers = _fixture.CreateMany<Supplier>(3).ToList();

            _suppliersRepositoryMock
                .Setup(r => r.GetAllSuppliers())
                .ReturnsAsync(suppliers);

            // Act
            var result = await _sut.GetAllSuppliers();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<SupplierResponse>();
            result.Select(r => r.SupplierID)
                  .Should().BeEquivalentTo(suppliers.Select(s => s.SupplierID));
        }

        [Fact]
        public async Task GetAllSuppliers_WithSuppliers_MapsFieldsCorrectly()
        {
            // Arrange – single supplier to verify field mapping precisely
            var supplier = _fixture.Create<Supplier>();

            _suppliersRepositoryMock
                .Setup(r => r.GetAllSuppliers())
                .ReturnsAsync(new List<Supplier> { supplier });

            // Act
            var result = await _sut.GetAllSuppliers();

            // Assert – all fields are correctly mapped to the response DTO
            var response = result.Single();
            response.SupplierID.Should().Be(supplier.SupplierID);
            response.SupplierName.Should().Be(supplier.SupplierName);
            response.ContactEmail.Should().Be(supplier.ContactEmail);
            response.LeadTime.Should().Be(supplier.LeadTime);
        }

        #endregion

        #region GetSupplierByID

        [Fact]
        public async Task GetSupplierByID_NullId_ThrowsArgumentNullException()
        {
            // Assert – system should throw exception when ID is null
            await _sut.Invoking(i => i.GetSupplierByID(null)).Should().ThrowAsync<ArgumentNullException>();
            _suppliersRepositoryMock.Verify(r => r.GetSupplierByID(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetSupplierByID_NonExistentId_ReturnsNull()
        {
            // Arrange – repository returns null for an unknown ID
            var nonExistentId = _fixture.Create<Guid>();

            _suppliersRepositoryMock
                .Setup(r => r.GetSupplierByID(nonExistentId))
                .ReturnsAsync((Supplier?)null);

            // Act
            var result = await _sut.GetSupplierByID(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSupplierByID_ValidId_ReturnsMatchingSupplierResponse()
        {
            // Arrange – AutoFixture creates a supplier with a known ID
            var supplier = _fixture.Create<Supplier>();

            _suppliersRepositoryMock
                .Setup(r => r.GetSupplierByID(supplier.SupplierID))
                .ReturnsAsync(supplier);

            // Act
            var result = await _sut.GetSupplierByID(supplier.SupplierID);

            // Assert – returned DTO matches the source entity
            result.Should().NotBeNull();
            result!.SupplierID.Should().Be(supplier.SupplierID);
            result.SupplierName.Should().Be(supplier.SupplierName);
            result.ContactEmail.Should().Be(supplier.ContactEmail);
            result.LeadTime.Should().Be(supplier.LeadTime);
        }

        [Fact]
        public async Task GetSupplierByID_ValidId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();

            _suppliersRepositoryMock
                .Setup(r => r.GetSupplierByID(supplier.SupplierID))
                .ReturnsAsync(supplier);

            // Act
            await _sut.GetSupplierByID(supplier.SupplierID);

            // Assert – repository should be called exactly once with the correct ID
            _suppliersRepositoryMock.Verify(r => r.GetSupplierByID(supplier.SupplierID), Times.Once);
        }

        #endregion
    }
}
