using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.Services.CustomerServices;

namespace WholesalerManager.ServiceTests.CustomerServiceTests
{
    public class CustomersDeleterServiceTests
    {
        private readonly Mock<ICustomersRepository> _customersRepositoryMock;
        private readonly Mock<ILogger<CustomersDeleterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly ICustomersDeleterService _sut;

        public CustomersDeleterServiceTests()
        {
            _customersRepositoryMock = new Mock<ICustomersRepository>();
            _loggerMock = new Mock<ILogger<CustomersDeleterService>>();

            _fixture = new Fixture();

            _sut = new CustomersDeleterService(
                _customersRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid CustomerDeleteRequest with all required fields filled
        private CustomerDeleteRequest CreateValidDeleteRequest(Action<CustomerDeleteRequest>? configure = null)
        {
            var request = _fixture.Build<CustomerDeleteRequest>()
                .With(r => r.CustomerID, Guid.NewGuid())
                .Create();

            configure?.Invoke(request);
            return request;
        }

        #endregion

        #region DeleteCustomer

        [Fact]
        public async Task DeleteCustomer_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any repository call
            await _sut.Invoking(s => s.DeleteCustomer(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _customersRepositoryMock.Verify(r => r.DeleteCustomer(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_EmptyCustomerID_ThrowsArgumentException()
        {
            // Arrange – CustomerID is required; Guid.Empty should fail validation
            var request = CreateValidDeleteRequest(r => r.CustomerID = Guid.Empty);

            // Act & Assert
            await _sut.Invoking(s => s.DeleteCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersRepositoryMock.Verify(r => r.DeleteCustomer(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCustomer_NonExistentCustomer_ReturnsFalse()
        {
            // Arrange – repository returns false when customer is not found
            var request = CreateValidDeleteRequest();

            _customersRepositoryMock
                .Setup(r => r.DeleteCustomer(request.CustomerID))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteCustomer(request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteCustomer_ExistingCustomer_ReturnsTrue()
        {
            // Arrange – repository confirms successful deletion
            var request = CreateValidDeleteRequest();

            _customersRepositoryMock
                .Setup(r => r.DeleteCustomer(request.CustomerID))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteCustomer(request);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteCustomer_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidDeleteRequest();

            _customersRepositoryMock
                .Setup(r => r.DeleteCustomer(request.CustomerID))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteCustomer(request);

            // Assert – repository should be called exactly once with the correct ID
            _customersRepositoryMock.Verify(r => r.DeleteCustomer(request.CustomerID), Times.Once);
        }

        #endregion
    }
}
