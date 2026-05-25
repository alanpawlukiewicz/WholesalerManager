using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.UI.Controllers;
using WholesalerManager.UI.ViewModels;

namespace WholesalerManager.ControllerTests
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrdersGetterService> _ordersGetterServiceMock;
        private readonly Mock<IOrdersDeleterService> _ordersDeleterServiceMock;
        private readonly Mock<IOrdersUpdaterService> _ordersUpdaterServiceMock;
        private readonly Mock<IOrderRegistrationService> _orderRegistrationServiceMock;
        private readonly Mock<IOrderUpdateCoordinatorService> _orderUpdateCoordinatorServiceMock;
        private readonly Mock<IOrderItemsGetterService> _orderItemsGetterServiceMock;
        private readonly Mock<IProductsGetterService> _productsGetterServiceMock;
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<IOrdersAdderService> _ordersAdderServiceMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly OrdersController _sut;

        public OrdersControllerTests()
        {
            _ordersGetterServiceMock = new Mock<IOrdersGetterService>();
            _ordersDeleterServiceMock = new Mock<IOrdersDeleterService>();
            _ordersUpdaterServiceMock = new Mock<IOrdersUpdaterService>();
            _orderRegistrationServiceMock = new Mock<IOrderRegistrationService>();
            _orderUpdateCoordinatorServiceMock = new Mock<IOrderUpdateCoordinatorService>();
            _orderItemsGetterServiceMock = new Mock<IOrderItemsGetterService>();
            _productsGetterServiceMock = new Mock<IProductsGetterService>();
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _ordersAdderServiceMock = new Mock<IOrdersAdderService>();
            _loggerMock = new Mock<ILogger<OrdersController>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrdersController(
                _ordersGetterServiceMock.Object,
                _ordersAdderServiceMock.Object,
                _ordersDeleterServiceMock.Object,
                _ordersUpdaterServiceMock.Object,
                _orderRegistrationServiceMock.Object,
                _orderUpdateCoordinatorServiceMock.Object,
                _orderItemsGetterServiceMock.Object,
                _productsGetterServiceMock.Object,
                _customersGetterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Helpers

        // Sets up TempData for actions that use TempData
        private void SetupTempData()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_NoFilterNoSort_CallsGetAllOrdersAndReturnsView()
        {
            // Arrange – no filter or sort parameters provided
            var orders = _fixture.CreateMany<OrderResponse>(3).ToList();
            var orderItems = _fixture.CreateMany<OrderItemResponse>(5).ToList();

            _ordersGetterServiceMock
                .Setup(s => s.GetAllOrders())
                .ReturnsAsync(orders);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetAllOrderItems())
                .ReturnsAsync(orderItems);

            // Act
            var result = await _sut.Index(null, null, null, null);

            // Assert – GetAllOrders called, view returned with correct model type
            _ordersGetterServiceMock.Verify(s => s.GetAllOrders(), Times.Once);
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<List<OrderWithProductsViewModel>>();
        }

        [Fact]
        public async Task Index_WithFilterAndPropertyName_CallsGetFilteredOrders()
        {
            // Arrange – both filter and propertyName provided
            var orders = _fixture.CreateMany<OrderResponse>(2).ToList();
            var orderItems = _fixture.CreateMany<OrderItemResponse>(3).ToList();

            _ordersGetterServiceMock
                .Setup(s => s.GetFilteredOrders(
                    nameof(OrderResponse.CustomerName), "Acme", false))
                .ReturnsAsync(orders);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetAllOrderItems())
                .ReturnsAsync(orderItems);

            // Act
            var result = await _sut.Index(nameof(OrderResponse.CustomerName), "Acme", false, null);

            // Assert – GetFilteredOrders called with correct arguments
            _ordersGetterServiceMock.Verify(s => s.GetFilteredOrders(
                nameof(OrderResponse.CustomerName), "Acme", false), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<List<OrderWithProductsViewModel>>();
        }

        [Fact]
        public async Task Index_WithSortOrderAndPropertyName_CallsGetSortedOrders()
        {
            // Arrange – sortOrder and propertyName provided, no filter
            var orders = _fixture.CreateMany<OrderResponse>(3).ToList();
            var orderItems = _fixture.CreateMany<OrderItemResponse>(3).ToList();

            _ordersGetterServiceMock
                .Setup(s => s.GetSortedOrders(
                    nameof(OrderResponse.CustomerName), SortOrderOptions.ASC))
                .ReturnsAsync(orders);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetAllOrderItems())
                .ReturnsAsync(orderItems);

            // Act
            var result = await _sut.Index(nameof(OrderResponse.CustomerName), null, null, SortOrderOptions.ASC);

            // Assert
            _ordersGetterServiceMock.Verify(s => s.GetSortedOrders(
                nameof(OrderResponse.CustomerName), SortOrderOptions.ASC), Times.Once);
        }

        [Fact]
        public async Task Index_SetsViewBagFieldNames()
        {
            // Arrange
            _ordersGetterServiceMock
                .Setup(s => s.GetAllOrders())
                .ReturnsAsync(new List<OrderResponse>());

            _orderItemsGetterServiceMock
                .Setup(s => s.GetAllOrderItems())
                .ReturnsAsync(new List<OrderItemResponse>());

            // Act
            await _sut.Index(null, null, null, null);

            // Assert – ViewBag.FieldNames contains the expected property names
            var fieldNames = _sut.ViewBag.FieldNames as List<string>;
            fieldNames.Should().NotBeNull();
            fieldNames.Should().Contain(nameof(OrderResponse.CustomerName));
            fieldNames.Should().Contain(nameof(OrderResponse.OrderDate));
            fieldNames.Should().Contain(nameof(OrderResponse.Status));
            fieldNames.Should().Contain(nameof(OrderResponse.TIN));
        }

        [Fact]
        public async Task Index_BuildsViewModelWithMatchingOrderItems()
        {
            // Arrange – order items are matched to orders by OrderID
            var order = _fixture.Create<OrderResponse>();
            var matchingItem = _fixture.Build<OrderItemResponse>()
                .With(i => i.OrderID, order.OrderID)
                .Create();
            var nonMatchingItem = _fixture.Create<OrderItemResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetAllOrders())
                .ReturnsAsync(new List<OrderResponse> { order });

            _orderItemsGetterServiceMock
                .Setup(s => s.GetAllOrderItems())
                .ReturnsAsync(new List<OrderItemResponse> { matchingItem, nonMatchingItem });

            // Act
            var result = await _sut.Index(null, null, null, null);

            // Assert – only matching items are included in the view model
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<List<OrderWithProductsViewModel>>().Subject;
            model.Should().HaveCount(1);
            model.First().Items.Should().HaveCount(1);
            model.First().Items.First().OrderID.Should().Be(order.OrderID);
        }

        #endregion

        #region Create GET

        [Fact]
        public async Task Create_Get_ReturnsViewWithCustomers()
        {
            // Arrange – customers loaded into ViewBag for the create form
            var customers = _fixture.CreateMany<CustomerResponse>(3).ToList();

            _customersGetterServiceMock
                .Setup(s => s.GetAllCustomers())
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.Create();

            // Assert – view returned and ViewBag.Customers populated
            result.Should().BeOfType<ViewResult>();
            var viewBagCustomers = _sut.ViewBag.Customers as IEnumerable<SelectListItem>;
            viewBagCustomers.Should().NotBeNull();
            viewBagCustomers!.Should().HaveCount(3);
        }

        #endregion

        #region Create POST

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithViewModel()
        {
            // Arrange – simulate invalid model state
            var viewModel = _fixture.Create<RegisterOrderViewModel>();
            _sut.ModelState.AddModelError("CustomerID", "Customer is required.");

            _customersGetterServiceMock
                .Setup(s => s.GetAllCustomers())
                .ReturnsAsync(_fixture.CreateMany<CustomerResponse>(3).ToList());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert – view returned, registration service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(viewModel);
            _orderRegistrationServiceMock.Verify(
                s => s.RegisterFullOrder(It.IsAny<OrderAddRequest>(), It.IsAny<List<OrderItemAddRequest>>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_NullViewModel_RedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            // Act
            var result = await _sut.Create(null!);

            // Assert – redirected to Index when view model is null
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Orders");
        }

        [Fact]
        public async Task Create_Post_ValidViewModel_RegistersOrderAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var viewModel = new RegisterOrderViewModel
            {
                OrderAddRequest = _fixture.Build<OrderAddRequest>()
                    .With(r => r.CustomerID, Guid.NewGuid())
                    .With(r => r.OrderDate, DateTime.UtcNow)
                    .Create(),
                Items = _fixture.CreateMany<OrderItemAddRequest>(2).ToList()
            };

            _orderRegistrationServiceMock
                .Setup(s => s.RegisterFullOrder(viewModel.OrderAddRequest, viewModel.Items))
                .ReturnsAsync(_fixture.Create<OrderResponse>());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            _orderRegistrationServiceMock.Verify(
                s => s.RegisterFullOrder(viewModel.OrderAddRequest, viewModel.Items), Times.Once);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Orders");
        }

        #endregion

        #region Update GET

        [Fact]
        public async Task Update_Get_ReturnsViewWithUpdateViewModel()
        {
            // Arrange
            var order = _fixture.Create<OrderResponse>();
            var orderItems = _fixture.CreateMany<OrderItemResponse>(2).ToList();
            var customers = _fixture.CreateMany<CustomerResponse>(3).ToList();
            var products = _fixture.CreateMany<ProductResponse>(3).ToList();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrderByID(order.OrderID))
                .ReturnsAsync(order);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetAllOrderItemsFromOrder(order.OrderID))
                .ReturnsAsync(orderItems);

            _customersGetterServiceMock
                .Setup(s => s.GetAllCustomers())
                .ReturnsAsync(customers);

            _productsGetterServiceMock
                .Setup(s => s.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.Update(order.OrderID);

            // Assert – view returned with UpdateOrderWithProductsViewModel
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<UpdateOrderWithProductsViewModel>().Subject;
            model.Order!.OrderID.Should().Be(order.OrderID);
            model.Items.Should().HaveCount(2);
        }

        #endregion

        #region Update POST

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithViewModel()
        {
            // Arrange
            var viewModel = _fixture.Create<UpdateOrderWithProductsViewModel>();
            _sut.ModelState.AddModelError("CustomerID", "Customer is required.");

            _customersGetterServiceMock
                .Setup(s => s.GetAllCustomers())
                .ReturnsAsync(_fixture.CreateMany<CustomerResponse>(3).ToList());

            // Act
            var result = await _sut.Update(viewModel);

            // Assert – view returned, coordinator service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(viewModel);
            _orderUpdateCoordinatorServiceMock.Verify(
                s => s.UpdateFullOrder(It.IsAny<OrderUpdateRequest>(), It.IsAny<List<OrderItemUpdateRequest>>()),
                Times.Never);
        }

        [Fact]
        public async Task Update_Post_ValidViewModel_UpdatesOrderAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var viewModel = new UpdateOrderWithProductsViewModel
            {
                Order = _fixture.Build<OrderUpdateRequest>()
                    .With(r => r.CustomerID, Guid.NewGuid())
                    .With(r => r.OrderDate, DateTime.UtcNow)
                    .With(r => r.Status, OrderStatus.PENDING)
                    .Create(),
                Items = _fixture.CreateMany<OrderItemUpdateRequest>(2).ToList()
            };

            _orderUpdateCoordinatorServiceMock
                .Setup(s => s.UpdateFullOrder(viewModel.Order, viewModel.Items))
                .ReturnsAsync(_fixture.Create<OrderResponse>());

            // Act
            var result = await _sut.Update(viewModel);

            // Assert
            _orderUpdateCoordinatorServiceMock.Verify(
                s => s.UpdateFullOrder(viewModel.Order, viewModel.Items), Times.Once);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Orders");
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_EmptyGuid_ReturnsErrorPartialView()
        {
            // Act
            var result = await _sut.Delete(Guid.Empty);

            // Assert – error toast returned, deleter service never called
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
            _ordersDeleterServiceMock.Verify(
                s => s.DeleteOrderByID(It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ValidGuid_DeletesOrderAndReturnsInfoPartialView()
        {
            // Arrange
            var orderId = _fixture.Create<Guid>();

            _ordersDeleterServiceMock
                .Setup(s => s.DeleteOrderByID(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(orderId);

            // Assert
            _ordersDeleterServiceMock.Verify(s => s.DeleteOrderByID(orderId), Times.Once);
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_infoToastPartialView");
        }

        #endregion

        #region CancelOrder

        [Fact]
        public async Task CancelOrder_ServiceReturnsFalse_RedirectsToIndexWithError()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var orderId = _fixture.Create<Guid>();

            _ordersUpdaterServiceMock
                .Setup(s => s.CancelOrder(orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.CancelOrder(orderId);

            // Assert – redirected to Index with error message in TempData
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Orders");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task CancelOrder_ServiceReturnsTrue_RedirectsToIndexWithSuccess()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var orderId = _fixture.Create<Guid>();

            _ordersUpdaterServiceMock
                .Setup(s => s.CancelOrder(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CancelOrder(orderId);

            // Assert – redirected to Index with success message in TempData
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Orders");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        #endregion

        #region UpdateOrderStatus

        [Fact]
        public async Task UpdateOrderStatus_ServiceReturnsFalse_ReturnsErrorPartialView()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var orderId = _fixture.Create<Guid>();

            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrderStatus(orderId, OrderStatus.PAID))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.PAID);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
        }

        [Fact]
        public async Task UpdateOrderStatus_ServiceReturnsTrue_ReturnsInfoPartialView()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var orderId = _fixture.Create<Guid>();

            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrderStatus(orderId, OrderStatus.PROCESSING))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.PROCESSING);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_infoToastPartialView");
        }

        #endregion
    }
}
