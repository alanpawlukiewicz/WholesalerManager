using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.UI.Controllers;

namespace WholesalerManager.ControllerTests
{
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<ICustomersAdderService> _customersAdderServiceMock;
        private readonly Mock<ICustomersUpdaterService> _customersUpdaterServiceMock;
        private readonly Mock<ICustomersDeleterService> _customersDeleterServiceMock;
        private readonly Mock<ILogger<CustomersController>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly CustomersController _sut;

        public CustomersControllerTests()
        {
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _customersAdderServiceMock = new Mock<ICustomersAdderService>();
            _customersUpdaterServiceMock = new Mock<ICustomersUpdaterService>();
            _customersDeleterServiceMock = new Mock<ICustomersDeleterService>();
            _loggerMock = new Mock<ILogger<CustomersController>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new CustomersController(
                _customersGetterServiceMock.Object,
                _customersAdderServiceMock.Object,
                _customersUpdaterServiceMock.Object,
                _customersDeleterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        private void SetupTempData()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);
        }

        private CustomerAddRequest CreateValidAddRequest(Action<CustomerAddRequest>? configure = null)
        {
            var request = _fixture.Build<CustomerAddRequest>()
                .With(r => r.CustomerName, "Acme Corp")
                .With(r => r.TIN, "123456789")
                .With(r => r.ContactEmail, "contact@acme.com")
                .With(r => r.Address, "123 Main Street")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        private CustomerUpdateRequest CreateValidUpdateRequest(Action<CustomerUpdateRequest>? configure = null)
        {
            var request = _fixture.Build<CustomerUpdateRequest>()
                .With(r => r.CustomerID, Guid.NewGuid())
                .With(r => r.CustomerName, "Acme Corp")
                .With(r => r.TIN, "123456789")
                .With(r => r.ContactEmail, "contact@acme.com")
                .With(r => r.Address, "123 Main Street")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_NoFilterNoSort_CallsGetAllCustomersAndReturnsView()
        {
            // Arrange – no filter or sort parameters provided
            var customers = _fixture.CreateMany<CustomerResponse>(3).ToList();

            _customersGetterServiceMock
                .Setup(s => s.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.Index(null, null, null, null);

            // Assert – GetAllCustomers called, view returned with correct model
            _customersGetterServiceMock.Verify(s => s.GetAllCustomers(), Times.Once);
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(customers);
        }

        [Fact]
        public async Task Index_WithFilterAndPropertyName_CallsGetFilteredCustomers()
        {
            // Arrange – both filter and propertyName provided
            var customers = _fixture.CreateMany<CustomerResponse>(2).ToList();

            _customersGetterServiceMock
                .Setup(s => s.GetFilteredCustomers(
                    nameof(CustomerResponse.CustomerName), "Acme", false))
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.Index(nameof(CustomerResponse.CustomerName), "Acme", false, null);

            // Assert – GetFilteredCustomers called with correct arguments
            _customersGetterServiceMock.Verify(s => s.GetFilteredCustomers(
                nameof(CustomerResponse.CustomerName), "Acme", false), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(customers);
        }

        [Fact]
        public async Task Index_WithSortOrderAndPropertyName_CallsGetSortedCustomers()
        {
            // Arrange – sortOrder and propertyName provided, no filter
            var customers = _fixture.CreateMany<CustomerResponse>(3).ToList();

            _customersGetterServiceMock
                .Setup(s => s.GetSortedCustomers(
                    nameof(CustomerResponse.CustomerName), SortOrderOptions.ASC))
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.Index(nameof(CustomerResponse.CustomerName), null, null, SortOrderOptions.ASC);

            // Assert
            _customersGetterServiceMock.Verify(s => s.GetSortedCustomers(
                nameof(CustomerResponse.CustomerName), SortOrderOptions.ASC), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(customers);
        }

        [Fact]
        public async Task Index_SetsViewBagFieldNamesWithoutCustomerID()
        {
            // Arrange
            _customersGetterServiceMock
                .Setup(s => s.GetAllCustomers())
                .ReturnsAsync(new List<CustomerResponse>());

            // Act
            await _sut.Index(null, null, null, null);

            // Assert – ViewBag.FieldNames excludes CustomerID
            var fieldNames = _sut.ViewBag.FieldNames as List<string>;
            fieldNames.Should().NotBeNull();
            fieldNames.Should().NotContain(nameof(CustomerResponse.CustomerID));
            fieldNames.Should().Contain(nameof(CustomerResponse.CustomerName));
            fieldNames.Should().Contain(nameof(CustomerResponse.TIN));
            fieldNames.Should().Contain(nameof(CustomerResponse.ContactEmail));
            fieldNames.Should().Contain(nameof(CustomerResponse.Address));
        }

        #endregion

        #region Create GET

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _sut.Create();

            // Assert – simple view returned with no model
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Create POST

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithRequest()
        {
            // Arrange – simulate invalid model state
            var request = CreateValidAddRequest();
            _sut.ModelState.AddModelError("CustomerName", "Customer name is required.");

            // Act
            var result = await _sut.Create(request);

            // Assert – view returned with the original request, adder service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(request);
            _customersAdderServiceMock.Verify(s => s.AddCustomer(It.IsAny<CustomerAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_ValidRequest_AdderReturnsFalse_RedirectsWithErrorMessage()
        {
            // Arrange – service signals TIN conflict or other failure
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = CreateValidAddRequest();

            _customersAdderServiceMock
                .Setup(s => s.AddCustomer(request))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Create(request);

            // Assert – redirected to Index with error message in TempData
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Create_Post_ValidRequest_AdderReturnsTrue_RedirectsWithInfoMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = CreateValidAddRequest();

            _customersAdderServiceMock
                .Setup(s => s.AddCustomer(request))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Create(request);

            // Assert – redirected to Index with success message in TempData
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        #endregion

        #region Update GET

        [Fact]
        public async Task Update_Get_NonExistentCustomer_RedirectsToIndex()
        {
            // Arrange – customer not found
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var customerId = _fixture.Create<Guid>();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(customerId))
                .ReturnsAsync((CustomerResponse?)null);

            // Act
            var result = await _sut.Update(customerId);

            // Assert – redirected to Index when customer is not found
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Update_Get_ExistingCustomer_ReturnsViewWithUpdateRequest()
        {
            // Arrange
            var customer = _fixture.Create<CustomerResponse>();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(customer.CustomerID))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.Update(customer.CustomerID);

            // Assert – view returned with CustomerUpdateRequest mapped from the customer
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<CustomerUpdateRequest>().Subject;
            model.CustomerID.Should().Be(customer.CustomerID);
            model.CustomerName.Should().Be(customer.CustomerName);
        }

        #endregion

        #region Update POST

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithRequest()
        {
            // Arrange
            var request = CreateValidUpdateRequest();
            _sut.ModelState.AddModelError("CustomerName", "Customer name is required.");

            // Act
            var result = await _sut.Update(request);

            // Assert – view returned, updater service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(request);
            _customersUpdaterServiceMock.Verify(s => s.UpdateCustomer(It.IsAny<CustomerUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task Update_Post_ValidRequest_UpdaterReturnsFalse_RedirectsWithErrorMessage()
        {
            // Arrange – service signals TIN conflict or customer not found
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = CreateValidUpdateRequest();

            _customersUpdaterServiceMock
                .Setup(s => s.UpdateCustomer(request))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Update(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Update_Post_ValidRequest_UpdaterReturnsTrue_RedirectsWithInfoMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = CreateValidUpdateRequest();

            _customersUpdaterServiceMock
                .Setup(s => s.UpdateCustomer(request))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Update(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        #endregion

        #region Delete GET

        [Fact]
        public async Task Delete_Get_NonExistentCustomer_RedirectsToIndex()
        {
            // Arrange – customer not found
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var customerId = _fixture.Create<Guid>();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(customerId))
                .ReturnsAsync((CustomerResponse?)null);

            // Act
            var result = await _sut.Delete(customerId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_Get_ExistingCustomer_ReturnsViewWithDeleteRequest()
        {
            // Arrange
            var customer = _fixture.Create<CustomerResponse>();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomerByID(customer.CustomerID))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.Delete(customer.CustomerID);

            // Assert – view returned with CustomerDeleteRequest mapped from the customer
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<CustomerDeleteRequest>().Subject;
            model.CustomerID.Should().Be(customer.CustomerID);
        }

        #endregion

        #region Delete POST

        [Fact]
        public async Task Delete_Post_InvalidModelState_ReturnsViewWithRequest()
        {
            // Arrange
            var request = _fixture.Create<CustomerDeleteRequest>();
            _sut.ModelState.AddModelError("CustomerID", "Customer ID is required.");

            // Act
            var result = await _sut.Delete(request);

            // Assert – view returned, deleter service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(request);
            _customersDeleterServiceMock.Verify(s => s.DeleteCustomer(It.IsAny<CustomerDeleteRequest>()), Times.Never);
        }

        [Fact]
        public async Task Delete_Post_ValidRequest_DeleterReturnsFalse_RedirectsWithErrorMessage()
        {
            // Arrange – service signals customer not found
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = _fixture.Create<CustomerDeleteRequest>();

            _customersDeleterServiceMock
                .Setup(s => s.DeleteCustomer(request))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Delete(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_Post_ValidRequest_DeleterReturnsTrue_RedirectsWithInfoMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = _fixture.Create<CustomerDeleteRequest>();

            _customersDeleterServiceMock
                .Setup(s => s.DeleteCustomer(request))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(request);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Customers");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        #endregion
    }
}
