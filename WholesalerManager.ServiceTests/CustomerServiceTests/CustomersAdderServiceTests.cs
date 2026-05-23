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

    public class CustomersAdderServiceTests
    {
        private readonly Mock<ICustomersRepository> _customersRepositoryMock;
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<ILogger<CustomersAdderService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly ICustomersAdderService _sut;

        public CustomersAdderServiceTests()
        {
            _customersRepositoryMock = new Mock<ICustomersRepository>();
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _loggerMock = new Mock<ILogger<CustomersAdderService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new CustomersAdderService(
                _customersRepositoryMock.Object,
                _customersGetterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a valid CustomerAddRequest with all required fields filled
        private CustomerAddRequest CreateValidAddRequest(Action<CustomerAddRequest>? configure = null)
        {
            var request = _fixture.Build<CustomerAddRequest>()
                .With(r => r.CustomerName, "Acme Corp")
                .With(r => r.TIN, "123456789")        // exactly 9 characters
                .With(r => r.ContactEmail, "contact@acme.com")
                .With(r => r.Address, "123 Main Street")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Sets up repository to return a Customer on AddNewCustomer call
        private void SetupRepositoryAdd()
        {
            _customersRepositoryMock
                .Setup(r => r.AddNewCustomer(It.IsAny<Customer>()))
                .ReturnsAsync((Customer c) => c);
        }

        #endregion

        #region AddCustomer

        [Fact]
        public async Task AddCustomer_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any service or repository call
            await _sut.Invoking(s => s.AddCustomer(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_MissingCustomerName_ThrowsArgumentException()
        {
            // Arrange – CustomerName is required by validation
            var request = CreateValidAddRequest(r => r.CustomerName = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_MissingContactEmail_ThrowsArgumentException()
        {
            // Arrange – ContactEmail is required by validation
            var request = CreateValidAddRequest(r => r.ContactEmail = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_InvalidContactEmail_ThrowsArgumentException()
        {
            // Arrange – ContactEmail must be a valid email address format
            var request = CreateValidAddRequest(r => r.ContactEmail = "not-an-email");

            // Act & Assert
            await _sut.Invoking(s => s.AddCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_MissingAddress_ThrowsArgumentException()
        {
            // Arrange – Address is required by validation
            var request = CreateValidAddRequest(r => r.Address = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Theory]
        [InlineData("12345678")]   // too short – 8 characters
        [InlineData("1234567890")] // too long – 10 characters
        public async Task AddCustomer_InvalidTINLength_ThrowsArgumentException(string tin)
        {
            // Arrange – TIN must be exactly 9 characters
            var request = CreateValidAddRequest(r => r.TIN = tin);

            // Act & Assert
            await _sut.Invoking(s => s.AddCustomer(request))
                      .Should().ThrowAsync<ArgumentException>();

            _customersGetterServiceMock.Verify(s => s.GetCustomerByTIN(It.IsAny<string?>()), Times.Never);
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_DuplicateTIN_ReturnsFalse()
        {
            // Arrange – another customer with the same TIN already exists
            var request = CreateValidAddRequest();
            var existingCustomer = _fixture.Create<CustomerResponse>();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(request.TIN))
                .ReturnsAsync(existingCustomer);

            // Act
            var result = await _sut.AddCustomer(request);

            // Assert – should return false without adding to repository
            result.Should().BeFalse();
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomer_UniqueTIN_ReturnsTrueAndCallsRepository()
        {
            // Arrange – no existing customer with this TIN
            var request = CreateValidAddRequest();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(request.TIN))
                .ReturnsAsync((CustomerResponse?)null);

            SetupRepositoryAdd();

            // Act
            var result = await _sut.AddCustomer(request);

            // Assert
            result.Should().BeTrue();
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task AddCustomer_NullTIN_AddsCustomerSuccessfully()
        {
            // Arrange – TIN is optional; null passes validation and uniqueness check returns null
            var request = CreateValidAddRequest(r => r.TIN = null);

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(null))
                .ReturnsAsync((CustomerResponse?)null);

            SetupRepositoryAdd();

            // Act
            var result = await _sut.AddCustomer(request);

            // Assert
            result.Should().BeTrue();
            _customersRepositoryMock.Verify(r => r.AddNewCustomer(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task AddCustomer_ValidRequest_PassesCustomerWithNewGuidToRepository()
        {
            // Arrange
            var request = CreateValidAddRequest();
            Customer? capturedCustomer = null;

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByTIN(request.TIN))
                .ReturnsAsync((CustomerResponse?)null);

            // Capture the customer passed to the repository to verify its contents
            _customersRepositoryMock
                .Setup(r => r.AddNewCustomer(It.IsAny<Customer>()))
                .Callback<Customer>(c => capturedCustomer = c)
                .ReturnsAsync((Customer c) => c);

            // Act
            await _sut.AddCustomer(request);

            // Assert – the customer passed to the repository should have a new non-empty ID
            capturedCustomer.Should().NotBeNull();
            capturedCustomer!.CustomerID.Should().NotBeEmpty();
            capturedCustomer.CustomerName.Should().Be(request.CustomerName);
            capturedCustomer.TIN.Should().Be(request.TIN);
            capturedCustomer.ContactEmail.Should().Be(request.ContactEmail);
            capturedCustomer.Address.Should().Be(request.Address);
        }

        #endregion
    }
}
