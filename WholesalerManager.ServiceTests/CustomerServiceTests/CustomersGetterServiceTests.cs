using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.Services.CustomerServices;
using Xunit;

namespace WholesalerManager.ServiceTests.CustomerServiceTests
{
    public class CustomersGetterServiceTests
    {
        private readonly Mock<ICustomersRepository> _customersRepositoryMock;
        private readonly Mock<ILogger<CustomersGetterService>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly ICustomersGetterService _sut;

        public CustomersGetterServiceTests()
        {
            _customersRepositoryMock = new Mock<ICustomersRepository>();
            _loggerMock = new Mock<ILogger<CustomersGetterService>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new CustomersGetterService(
                _customersRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Creates a Customer entity with AutoFixture
        private Customer CreateCustomer(Action<Customer>? configure = null)
        {
            var customer = _fixture.Create<Customer>();
            configure?.Invoke(customer);
            return customer;
        }

        #endregion

        #region GetAllCustomers

        [Fact]
        public async Task GetAllCustomers_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange – repository returns no customers
            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer>());

            // Act
            var result = await _sut.GetAllCustomers();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCustomers_WithCustomers_ReturnsAllMappedToCustomerResponse()
        {
            // Arrange – AutoFixture generates a list of random customers
            var customers = _fixture.CreateMany<Customer>(3).ToList();

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetAllCustomers();

            // Assert – count and IDs match the source entities
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<CustomerResponse>();
            result.Select(r => r.CustomerID)
                  .Should().BeEquivalentTo(customers.Select(c => c.CustomerID));
        }

        [Fact]
        public async Task GetAllCustomers_WithCustomers_MapsFieldsCorrectly()
        {
            // Arrange – single customer to verify field mapping precisely
            var customer = CreateCustomer();

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { customer });

            // Act
            var result = await _sut.GetAllCustomers();

            // Assert
            var response = result.Single();
            response.CustomerID.Should().Be(customer.CustomerID);
            response.CustomerName.Should().Be(customer.CustomerName);
            response.TIN.Should().Be(customer.TIN);
            response.ContactEmail.Should().Be(customer.ContactEmail);
            response.Address.Should().Be(customer.Address);
        }

        #endregion

        #region GetCustomerByID

        [Fact]
        public async Task GetCustomerByID_NullId_ThrowsArgumentNullException()
        {
            // Act & Assert – null ID should throw before any repository call
            await _sut.Invoking(s => s.GetCustomerByID(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _customersRepositoryMock.Verify(r => r.GetCustomerById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetCustomerByID_NonExistentId_ReturnsNull()
        {
            // Arrange – repository returns null for an unknown ID
            var nonExistentId = _fixture.Create<Guid>();

            _customersRepositoryMock
                .Setup(r => r.GetCustomerById(nonExistentId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _sut.GetCustomerByID(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomerByID_ValidId_ReturnsMatchingCustomerResponse()
        {
            // Arrange
            var customer = CreateCustomer();

            _customersRepositoryMock
                .Setup(r => r.GetCustomerById(customer.CustomerID))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.GetCustomerByID(customer.CustomerID);

            // Assert – returned DTO matches the source entity
            result.Should().NotBeNull();
            result!.CustomerID.Should().Be(customer.CustomerID);
            result.CustomerName.Should().Be(customer.CustomerName);
            result.TIN.Should().Be(customer.TIN);
            result.ContactEmail.Should().Be(customer.ContactEmail);
            result.Address.Should().Be(customer.Address);
        }

        [Fact]
        public async Task GetCustomerByID_ValidId_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var customer = CreateCustomer();

            _customersRepositoryMock
                .Setup(r => r.GetCustomerById(customer.CustomerID))
                .ReturnsAsync(customer);

            // Act
            await _sut.GetCustomerByID(customer.CustomerID);

            // Assert
            _customersRepositoryMock.Verify(r => r.GetCustomerById(customer.CustomerID), Times.Once);
        }

        #endregion

        #region GetCustomerByTIN

        [Fact]
        public async Task GetCustomerByTIN_NullTIN_ThrowsArgumentNullException()
        {
            // Act & Assert – null TIN should throw before any repository call
            await _sut.Invoking(s => s.GetCustomerByTIN(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _customersRepositoryMock.Verify(r => r.GetCustomerByTIN(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCustomerByTIN_NonExistentTIN_ReturnsNull()
        {
            // Arrange – repository returns null for an unknown TIN
            var tin = "123456789";

            _customersRepositoryMock
                .Setup(r => r.GetCustomerByTIN(tin))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _sut.GetCustomerByTIN(tin);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomerByTIN_ExistingTIN_ReturnsMatchingCustomerResponse()
        {
            // Arrange
            var customer = CreateCustomer(c => c.TIN = "123456789");

            _customersRepositoryMock
                .Setup(r => r.GetCustomerByTIN(customer.TIN!))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.GetCustomerByTIN(customer.TIN);

            // Assert
            result.Should().NotBeNull();
            result!.TIN.Should().Be(customer.TIN);
            result.CustomerID.Should().Be(customer.CustomerID);
            result.CustomerName.Should().Be(customer.CustomerName);
        }

        [Fact]
        public async Task GetCustomerByTIN_ValidTIN_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var tin = "123456789";

            _customersRepositoryMock
                .Setup(r => r.GetCustomerByTIN(tin))
                .ReturnsAsync((Customer?)null);

            // Act
            await _sut.GetCustomerByTIN(tin);

            // Assert
            _customersRepositoryMock.Verify(r => r.GetCustomerByTIN(tin), Times.Once);
        }

        #endregion

        #region GetFilteredCustomers

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("CustomerName", null)]
        [InlineData("CustomerName", "")]
        [InlineData(null, "filter")]
        public async Task GetFilteredCustomers_NullOrEmptyFilterOrProperty_ReturnsAllCustomers(
            string? propertyName, string? filter)
        {
            // Arrange – when filter or property name is empty, all customers should be returned
            var customers = _fixture.CreateMany<Customer>(3).ToList();

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetFilteredCustomers(propertyName, filter);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetFilteredCustomers_FilterByCustomerName_ReturnsMatchingCustomers()
        {
            // Arrange – two customers share a common name fragment, one does not
            var matching1 = CreateCustomer(c => c.CustomerName = "Acme Corp");
            var matching2 = CreateCustomer(c => c.CustomerName = "Acme Ltd");
            var nonMatching = CreateCustomer(c => c.CustomerName = "Other Company");

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { matching1, matching2, nonMatching });

            // Act – filter by "acme" (case-insensitive by default)
            var result = await _sut.GetFilteredCustomers(nameof(CustomerResponse.CustomerName), "acme");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.CustomerName!.Contains("Acme", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredCustomers_FilterByTIN_ReturnsMatchingCustomers()
        {
            // Arrange
            var matching1 = CreateCustomer(c => c.TIN = "123456789");
            var matching2 = CreateCustomer(c => c.TIN = "123000000");
            var nonMatching = CreateCustomer(c => c.TIN = "999999999");

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredCustomers(nameof(CustomerResponse.TIN), "123");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.TIN!.Contains("123"));
        }

        [Fact]
        public async Task GetFilteredCustomers_FilterByContactEmail_ReturnsMatchingCustomers()
        {
            // Arrange
            var matching1 = CreateCustomer(c => c.ContactEmail = "alice@acme.com");
            var matching2 = CreateCustomer(c => c.ContactEmail = "bob@acme.com");
            var nonMatching = CreateCustomer(c => c.ContactEmail = "carol@other.com");

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredCustomers(nameof(CustomerResponse.ContactEmail), "acme.com");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.ContactEmail!.Contains("acme.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredCustomers_FilterByAddress_ReturnsMatchingCustomers()
        {
            // Arrange
            var matching1 = CreateCustomer(c => c.Address = "123 Main Street");
            var matching2 = CreateCustomer(c => c.Address = "456 Main Avenue");
            var nonMatching = CreateCustomer(c => c.Address = "789 Other Road");

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { matching1, matching2, nonMatching });

            // Act
            var result = await _sut.GetFilteredCustomers(nameof(CustomerResponse.Address), "main");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.Address!.Contains("Main", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredCustomers_CaseSensitiveFilter_ReturnsOnlyExactCaseMatches()
        {
            // Arrange – same name, different casing
            var upperCase = CreateCustomer(c => c.CustomerName = "Acme Corp");
            var lowerCase = CreateCustomer(c => c.CustomerName = "acme corp");

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { upperCase, lowerCase });

            // Act – ignoreCase: false should only match exact casing
            var result = await _sut.GetFilteredCustomers(
                nameof(CustomerResponse.CustomerName), "Acme Corp", ignoreCase: false);

            // Assert
            result.Should().HaveCount(1);
            result.First().CustomerName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task GetFilteredCustomers_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { CreateCustomer() });

            // Act & Assert – an unsupported property name should throw ArgumentException
            await _sut.Invoking(s => s.GetFilteredCustomers("InvalidProperty", "someFilter"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*InvalidProperty*");
        }

        #endregion

        #region GetSortedCustomers

        [Fact]
        public async Task GetSortedCustomers_NullPropertyName_ReturnsCustomersInOriginalOrder()
        {
            // Arrange – without a sort property, original repository order should be preserved
            var customers = _fixture.CreateMany<Customer>(3).ToList();

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetSortedCustomers(null);

            // Assert – IDs match original order
            result.Select(r => r.CustomerID)
                  .Should().ContainInOrder(customers.Select(c => c.CustomerID));
        }

        [Fact]
        public async Task GetSortedCustomers_ByCustomerNameAscending_ReturnsSortedAscending()
        {
            // Arrange
            var customers = new List<Customer>
        {
            CreateCustomer(c => c.CustomerName = "Zebra Corp"),
            CreateCustomer(c => c.CustomerName = "Acme Ltd"),
            CreateCustomer(c => c.CustomerName = "Mango Inc"),
        };

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetSortedCustomers(nameof(CustomerResponse.CustomerName), SortOrderOptions.ASC);

            // Assert
            result.Select(c => c.CustomerName)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedCustomers_ByCustomerNameDescending_ReturnsSortedDescending()
        {
            // Arrange
            var customers = new List<Customer>
        {
            CreateCustomer(c => c.CustomerName = "Acme Ltd"),
            CreateCustomer(c => c.CustomerName = "Zebra Corp"),
            CreateCustomer(c => c.CustomerName = "Mango Inc"),
        };

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetSortedCustomers(nameof(CustomerResponse.CustomerName), SortOrderOptions.DESC);

            // Assert
            result.Select(c => c.CustomerName)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedCustomers_ByContactEmailAscending_ReturnsSortedAscending()
        {
            // Arrange
            var customers = new List<Customer>
        {
            CreateCustomer(c => c.ContactEmail = "zebra@mail.com"),
            CreateCustomer(c => c.ContactEmail = "acme@mail.com"),
            CreateCustomer(c => c.ContactEmail = "mango@mail.com"),
        };

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetSortedCustomers(nameof(CustomerResponse.ContactEmail), SortOrderOptions.ASC);

            // Assert
            result.Select(c => c.ContactEmail)
                  .Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetSortedCustomers_ByAddressDescending_ReturnsSortedDescending()
        {
            // Arrange
            var customers = new List<Customer>
        {
            CreateCustomer(c => c.Address = "123 Apple Street"),
            CreateCustomer(c => c.Address = "789 Zebra Avenue"),
            CreateCustomer(c => c.Address = "456 Mango Road"),
        };

            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetSortedCustomers(nameof(CustomerResponse.Address), SortOrderOptions.DESC);

            // Assert
            result.Select(c => c.Address)
                  .Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetSortedCustomers_InvalidPropertyName_ThrowsArgumentException()
        {
            // Arrange
            _customersRepositoryMock
                .Setup(r => r.GetAllCustomers())
                .ReturnsAsync(new List<Customer> { CreateCustomer() });

            // Act & Assert
            await _sut.Invoking(s => s.GetSortedCustomers("NonExistentProperty"))
                      .Should().ThrowAsync<ArgumentException>()
                      .WithMessage("*NonExistentProperty*");
        }

        #endregion
    }
}
