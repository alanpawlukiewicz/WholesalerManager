using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.Services.CustomerServices;

namespace WholesalerManager.ServiceTests.CustomerServiceTests
{
    public class CustomersUpdaterServiceTests
    {
        private readonly Mock<ICustomersRepository> _customersRepositoryMock;
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<ILogger<CustomersUpdaterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly ICustomersUpdaterService _sut;

        public CustomersUpdaterServiceTests()
        {
            _customersRepositoryMock = new Mock<ICustomersRepository>();
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _loggerMock = new Mock<ILogger<CustomersUpdaterService>>();

            _fixture = new Fixture();

            _sut = new CustomersUpdaterService(
                _customersRepositoryMock.Object,
                _customersGetterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid CustomerUpdateRequest with all required fields filled
        private CustomerUpdateRequest CreateValidUpdateRequest(Action<CustomerUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<CustomerUpdateRequest>()
                .With(r => r.CustomerID, Guid.NewGuid())
                .With(r => r.CustomerName, "Acme Corp")
                .With(r => r.TIN, "123456789")        // exactly 9 characters
                .With(r => r.ContactEmail, "contact@acme.com")
                .With(r => r.Address, "123 Main Street")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Sets up getter service to return null for TIN lookup (no duplicate)
        private void SetupNoTINConflict(string? tin)
        {
            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(tin))
                .ReturnsAsync((CustomerResponse?)null);
        }

        // Sets up repository to confirm successful update
        private void SetupRepositoryUpdateSuccess()
        {
            _customersRepositoryMock
                .Setup(r => r.UpdateCustomer(It.IsAny<Customer>()))
                .ReturnsAsync(true);
        }

        #endregion

        #region UpdateCustomer

        [Fact]
        public async Task UpdateCustomer_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any service or repository call
            await _sut.Invoking(s => s.UpdateCustomer(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_MissingCustomerName_ThrowsArgumentException()
        {
            // Arrange – CustomerName is required by validation
            var request = CreateValidUpdateRequest(r => r.CustomerName = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_MissingContactEmail_ThrowsArgumentException()
        {
            // Arrange – ContactEmail is required by validation
            var request = CreateValidUpdateRequest(r => r.ContactEmail = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_InvalidContactEmail_ThrowsArgumentException()
        {
            // Arrange – ContactEmail must be a valid email address format
            var request = CreateValidUpdateRequest(r => r.ContactEmail = "not-an-email");

            // Act & Assert
            await _sut.Invoking(s => s.UpdateCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_MissingAddress_ThrowsArgumentException()
        {
            // Arrange – Address is required by validation
            var request = CreateValidUpdateRequest(r => r.Address = null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Theory]
        [InlineData("12345678")]   // too short – 8 characters
        [InlineData("1234567890")] // too long – 10 characters
        public async Task UpdateCustomer_InvalidTINLength_ThrowsArgumentException(string tin)
        {
            // Arrange – TIN must be exactly 9 characters
            var request = CreateValidUpdateRequest(r => r.TIN = tin);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_TINBelongsToAnotherCustomer_ReturnsFalse()
        {
            // Arrange – TIN already exists but belongs to a different customer
            var request = CreateValidUpdateRequest();
            var conflictingCustomer = _fixture.Build<CustomerResponse>()
                .With(c => c.TIN, request.TIN)
                .With(c => c.CustomerID, Guid.NewGuid()) // different ID than request
                .Create();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(request.TIN))
                .ReturnsAsync(conflictingCustomer);

            // Act
            var result = await _sut.UpdateCustomer(request);

            // Assert – should return false without updating repository
            result.Should().BeFalse();
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_TINBelongsToSameCustomer_CallsRepositoryAndReturnsTrue()
        {
            // Arrange – TIN exists but belongs to the same customer being updated (valid scenario)
            var request = CreateValidUpdateRequest();
            var sameCustomer = _fixture.Build<CustomerResponse>()
                .With(c => c.TIN, request.TIN)
                .With(c => c.CustomerID, request.CustomerID) // same ID as request
                .Create();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(request.TIN))
                .ReturnsAsync(sameCustomer);

            SetupRepositoryUpdateSuccess();

            // Act
            var result = await _sut.UpdateCustomer(request);

            // Assert
            result.Should().BeTrue();
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_UniqueTIN_CallsRepositoryAndReturnsTrue()
        {
            // Arrange – no existing customer with this TIN
            var request = CreateValidUpdateRequest();

            SetupNoTINConflict(request.TIN);
            SetupRepositoryUpdateSuccess();

            // Act
            var result = await _sut.UpdateCustomer(request);

            // Assert
            result.Should().BeTrue();
            _customersRepositoryMock.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_RepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange – repository signals the customer was not found or update failed
            var request = CreateValidUpdateRequest();

            SetupNoTINConflict(request.TIN);

            _customersRepositoryMock
                .Setup(r => r.UpdateCustomer(It.IsAny<Customer>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.UpdateCustomer(request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateCustomer_ValidRequest_PassesCorrectCustomerToRepository()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            Customer? capturedCustomer = null;

            SetupNoTINConflict(request.TIN);

            // Capture the customer passed to the repository to verify its contents
            _customersRepositoryMock
                .Setup(r => r.UpdateCustomer(It.IsAny<Customer>()))
                .Callback<Customer>(c => capturedCustomer = c)
                .ReturnsAsync(true);

            // Act
            await _sut.UpdateCustomer(request);

            // Assert – the customer passed to the repository should match the request
            capturedCustomer.Should().NotBeNull();
            capturedCustomer!.CustomerID.Should().Be(request.CustomerID);
            capturedCustomer.CustomerName.Should().Be(request.CustomerName);
            capturedCustomer.TIN.Should().Be(request.TIN);
            capturedCustomer.ContactEmail.Should().Be(request.ContactEmail);
            capturedCustomer.Address.Should().Be(request.Address);
        }

        #endregion
    }
}
