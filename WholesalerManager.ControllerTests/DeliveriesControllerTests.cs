using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.DTO.SupplierDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.UI.Controllers;
using WholesalerManager.UI.ViewModels;
using Xunit;

namespace WholesalerManager.ControllerTests
{
    public class DeliveriesControllerTests
    {
        private readonly Mock<IDeliveriesGetterService> _deliveriesGetterServiceMock;
        private readonly Mock<IDeliveriesAdderService> _deliveriesAdderServiceMock;
        private readonly Mock<IDeliveriesUpdaterService> _deliveriesUpdaterServiceMock;
        private readonly Mock<IDeliveriesDeleterService> _deliveriesDeleterServiceMock;
        private readonly Mock<IDeliveryRegistrationService> _deliveryRegistrationServiceMock;
        private readonly Mock<IDeliveryUpdateCoordinatorService> _deliveryUpdateCoordinatorServiceMock;
        private readonly Mock<IDeliveryItemsAdderService> _deliveryItemsAdderServiceMock;
        private readonly Mock<IDeliveryItemsGetterService> _deliveryItemsGetterServiceMock;
        private readonly Mock<IDeliveryItemsUpdaterService> _deliveryItemsUpdaterServiceMock;
        private readonly Mock<ISuppliersGetterService> _suppliersGetterServiceMock;
        private readonly Mock<IProductsGetterService> _productsGetterServiceMock;
        private readonly Mock<ILogger<DeliveriesController>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly DeliveriesController _sut;

        public DeliveriesControllerTests()
        {
            _deliveriesGetterServiceMock = new Mock<IDeliveriesGetterService>();
            _deliveriesAdderServiceMock = new Mock<IDeliveriesAdderService>();
            _deliveriesUpdaterServiceMock = new Mock<IDeliveriesUpdaterService>();
            _deliveriesDeleterServiceMock = new Mock<IDeliveriesDeleterService>();
            _deliveryRegistrationServiceMock = new Mock<IDeliveryRegistrationService>();
            _deliveryUpdateCoordinatorServiceMock = new Mock<IDeliveryUpdateCoordinatorService>();
            _deliveryItemsAdderServiceMock = new Mock<IDeliveryItemsAdderService>();
            _deliveryItemsGetterServiceMock = new Mock<IDeliveryItemsGetterService>();
            _deliveryItemsUpdaterServiceMock = new Mock<IDeliveryItemsUpdaterService>();
            _suppliersGetterServiceMock = new Mock<ISuppliersGetterService>();
            _productsGetterServiceMock = new Mock<IProductsGetterService>();
            _loggerMock = new Mock<ILogger<DeliveriesController>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new DeliveriesController(
                _deliveriesGetterServiceMock.Object,
                _deliveriesAdderServiceMock.Object,
                _deliveriesUpdaterServiceMock.Object,
                _deliveriesDeleterServiceMock.Object,
                _deliveryRegistrationServiceMock.Object,
                _deliveryUpdateCoordinatorServiceMock.Object,
                _deliveryItemsAdderServiceMock.Object,
                _deliveryItemsGetterServiceMock.Object,
                _deliveryItemsUpdaterServiceMock.Object,
                _suppliersGetterServiceMock.Object,
                _productsGetterServiceMock.Object,
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

        #endregion

        #region Index

        [Fact]
        public async Task Index_NoFilterNoSort_CallsGetAllDeliveriesAndReturnsView()
        {
            // Arrange – no filter or sort parameters provided
            var deliveries = _fixture.CreateMany<DeliveryResponse>(3).ToList();
            var items = _fixture.CreateMany<DeliveryItemResponse>(5).ToList();

            _deliveriesGetterServiceMock
                .Setup(s => s.GetAllDeliveries())
                .ReturnsAsync(deliveries);

            _deliveryItemsGetterServiceMock
                .Setup(s => s.GetAllDeliveryItems())
                .ReturnsAsync(items);

            // Act
            var result = await _sut.Index(null, null, null, null);

            // Assert – GetAllDeliveries called, view returned with correct model type
            _deliveriesGetterServiceMock.Verify(s => s.GetAllDeliveries(), Times.Once);
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<List<DeliveryWithProductsViewModel>>();
        }

        [Fact]
        public async Task Index_WithFilterAndPropertyName_CallsGetFilteredDeliveries()
        {
            // Arrange – both filter and propertyName provided
            var deliveries = _fixture.CreateMany<DeliveryResponse>(2).ToList();
            var items = _fixture.CreateMany<DeliveryItemResponse>(3).ToList();

            _deliveriesGetterServiceMock
                .Setup(s => s.GetFilteredDeliveries(
                    nameof(DeliveryResponse.SupplierName), "Acme", false))
                .ReturnsAsync(deliveries);

            _deliveryItemsGetterServiceMock
                .Setup(s => s.GetAllDeliveryItems())
                .ReturnsAsync(items);

            // Act
            var result = await _sut.Index(nameof(DeliveryResponse.SupplierName), "Acme", false, null);

            // Assert – GetFilteredDeliveries called with correct arguments
            _deliveriesGetterServiceMock.Verify(s => s.GetFilteredDeliveries(
                nameof(DeliveryResponse.SupplierName), "Acme", false), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<List<DeliveryWithProductsViewModel>>();
        }

        [Fact]
        public async Task Index_WithSortOrderAndPropertyName_CallsGetSortedDeliveries()
        {
            // Arrange – sortOrder and propertyName provided, no filter
            var deliveries = _fixture.CreateMany<DeliveryResponse>(3).ToList();
            var items = _fixture.CreateMany<DeliveryItemResponse>(3).ToList();

            _deliveriesGetterServiceMock
                .Setup(s => s.GetSortedDeliveries(
                    nameof(DeliveryResponse.SupplierName), SortOrderOptions.ASC))
                .ReturnsAsync(deliveries);

            _deliveryItemsGetterServiceMock
                .Setup(s => s.GetAllDeliveryItems())
                .ReturnsAsync(items);

            // Act
            var result = await _sut.Index(nameof(DeliveryResponse.SupplierName), null, null, SortOrderOptions.ASC);

            // Assert
            _deliveriesGetterServiceMock.Verify(s => s.GetSortedDeliveries(
                nameof(DeliveryResponse.SupplierName), SortOrderOptions.ASC), Times.Once);
        }

        [Fact]
        public async Task Index_SetsViewBagFieldNames()
        {
            // Arrange
            _deliveriesGetterServiceMock
                .Setup(s => s.GetAllDeliveries())
                .ReturnsAsync(new List<DeliveryResponse>());

            _deliveryItemsGetterServiceMock
                .Setup(s => s.GetAllDeliveryItems())
                .ReturnsAsync(new List<DeliveryItemResponse>());

            // Act
            await _sut.Index(null, null, null, null);

            // Assert – ViewBag.FieldNames contains the expected property names
            var fieldNames = _sut.ViewBag.FieldNames as List<string>;
            fieldNames.Should().NotBeNull();
            fieldNames.Should().Contain(nameof(DeliveryResponse.SupplierName));
            fieldNames.Should().Contain(nameof(DeliveryResponse.Status));
            fieldNames.Should().Contain(nameof(DeliveryResponse.OrderDate));
        }

        [Fact]
        public async Task Index_BuildsViewModelWithMatchingDeliveryItems()
        {
            // Arrange – delivery items are matched to deliveries by DeliveryID
            var delivery = _fixture.Create<DeliveryResponse>();
            var matchingItem = _fixture.Build<DeliveryItemResponse>()
                .With(i => i.DeliveryID, delivery.DeliveryID)
                .Create();
            var nonMatchingItem = _fixture.Create<DeliveryItemResponse>();

            _deliveriesGetterServiceMock
                .Setup(s => s.GetAllDeliveries())
                .ReturnsAsync(new List<DeliveryResponse> { delivery });

            _deliveryItemsGetterServiceMock
                .Setup(s => s.GetAllDeliveryItems())
                .ReturnsAsync(new List<DeliveryItemResponse> { matchingItem, nonMatchingItem });

            // Act
            var result = await _sut.Index(null, null, null, null);

            // Assert – only matching items are included in the view model
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<List<DeliveryWithProductsViewModel>>().Subject;
            model.Should().HaveCount(1);
            model.First().Items.Should().HaveCount(1);
            model.First().Items.First().DeliveryID.Should().Be(delivery.DeliveryID);
        }

        #endregion

        #region Create GET

        [Fact]
        public async Task Create_Get_ReturnsViewWithSuppliers()
        {
            // Arrange – suppliers loaded into ViewBag for the create form
            var suppliers = _fixture.CreateMany<SupplierResponse>(3).ToList();

            _suppliersGetterServiceMock
                .Setup(s => s.GetAllSuppliers())
                .ReturnsAsync(suppliers);

            // Act
            var result = await _sut.Create();

            // Assert – view returned and ViewBag.Suppliers populated
            result.Should().BeOfType<ViewResult>();
            var viewBagSuppliers = _sut.ViewBag.Suppliers as IEnumerable<SelectListItem>;
            viewBagSuppliers.Should().NotBeNull();
            viewBagSuppliers!.Should().HaveCount(3);
        }

        #endregion

        #region Create POST

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithViewModel()
        {
            // Arrange – simulate invalid model state
            var viewModel = _fixture.Create<RegisterDeliveryViewModel>();
            _sut.ModelState.AddModelError("SupplierID", "Supplier is required.");

            _suppliersGetterServiceMock
                .Setup(s => s.GetAllSuppliers())
                .ReturnsAsync(_fixture.CreateMany<SupplierResponse>(3).ToList());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert – view returned, registration service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(viewModel);
            _deliveryRegistrationServiceMock.Verify(
                s => s.RegisterFullDelivery(It.IsAny<DeliveryAddRequest>(), It.IsAny<List<DeliveryItemAddRequest>>()),
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
            redirectResult.ControllerName.Should().Be("Deliveries");
        }

        [Fact]
        public async Task Create_Post_ValidViewModel_RegistersDeliveryAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var viewModel = new RegisterDeliveryViewModel
            {
                DeliveryAddRequest = _fixture.Build<DeliveryAddRequest>()
                    .With(r => r.SupplierID, Guid.NewGuid())
                    .With(r => r.OrderDate, DateTime.UtcNow)
                    .With(r => r.Status, DeliveryStatus.ORDERED)
                    .Create(),
                Items = _fixture.CreateMany<DeliveryItemAddRequest>(2).ToList()
            };

            _deliveryRegistrationServiceMock
                .Setup(s => s.RegisterFullDelivery(viewModel.DeliveryAddRequest, viewModel.Items))
                .ReturnsAsync(_fixture.Create<DeliveryResponse>());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            _deliveryRegistrationServiceMock.Verify(
                s => s.RegisterFullDelivery(viewModel.DeliveryAddRequest, viewModel.Items), Times.Once);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Deliveries");
        }

        #endregion

        #region Update GET

        [Fact]
        public async Task Update_Get_ReturnsViewWithUpdateViewModel()
        {
            // Arrange
            var delivery = _fixture.Create<DeliveryResponse>();
            var deliveryItems = _fixture.CreateMany<DeliveryItemResponse>(2).ToList();
            var suppliers = _fixture.CreateMany<SupplierResponse>(3).ToList();
            var products = _fixture.CreateMany<ProductResponse>(3).ToList();

            _deliveriesGetterServiceMock
                .Setup(s => s.GetDeliveryById(delivery.DeliveryID))
                .ReturnsAsync(delivery);

            _deliveryItemsGetterServiceMock
                .Setup(s => s.GetAllDeliveryItemsFromDelivery(delivery.DeliveryID))
                .ReturnsAsync(deliveryItems);

            _suppliersGetterServiceMock
                .Setup(s => s.GetAllSuppliers())
                .ReturnsAsync(suppliers);

            _productsGetterServiceMock
                .Setup(s => s.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.Update(delivery.DeliveryID);

            // Assert – view returned with UpdateDeliveryWithProductsViewModel
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<UpdateDeliveryWithProductsViewModel>().Subject;
            model.Delivery!.DeliveryID.Should().Be(delivery.DeliveryID);
            model.Items.Should().HaveCount(2);
        }

        #endregion

        #region Update POST

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithViewModel()
        {
            // Arrange
            var viewModel = _fixture.Create<UpdateDeliveryWithProductsViewModel>();
            _sut.ModelState.AddModelError("SupplierID", "Supplier is required.");

            _suppliersGetterServiceMock
                .Setup(s => s.GetAllSuppliers())
                .ReturnsAsync(_fixture.CreateMany<SupplierResponse>(3).ToList());

            // Act
            var result = await _sut.Update(viewModel);

            // Assert – view returned, coordinator service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(viewModel);
            _deliveryUpdateCoordinatorServiceMock.Verify(
                s => s.UpdateFullDelivery(It.IsAny<DeliveryUpdateRequest>(), It.IsAny<List<DeliveryItemUpdateRequest>>()),
                Times.Never);
        }

        [Fact]
        public async Task Update_Post_ValidViewModel_UpdatesDeliveryAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var viewModel = new UpdateDeliveryWithProductsViewModel
            {
                Delivery = _fixture.Build<DeliveryUpdateRequest>()
                    .With(r => r.DeliveryID, Guid.NewGuid())
                    .With(r => r.SupplierID, Guid.NewGuid())
                    .With(r => r.OrderDate, DateTime.UtcNow)
                    .With(r => r.Status, DeliveryStatus.ORDERED)
                    .Create(),
                Items = _fixture.CreateMany<DeliveryItemUpdateRequest>(2).ToList()
            };

            _deliveryUpdateCoordinatorServiceMock
                .Setup(s => s.UpdateFullDelivery(viewModel.Delivery, viewModel.Items))
                .ReturnsAsync(_fixture.Create<DeliveryResponse>());

            // Act
            var result = await _sut.Update(viewModel);

            // Assert
            _deliveryUpdateCoordinatorServiceMock.Verify(
                s => s.UpdateFullDelivery(viewModel.Delivery, viewModel.Items), Times.Once);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Deliveries");
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
            _deliveriesDeleterServiceMock.Verify(
                s => s.DeleteDeliveryByID(It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ValidGuid_DeletesDeliveryAndReturnsInfoPartialView()
        {
            // Arrange
            var deliveryId = _fixture.Create<Guid>();

            _deliveriesDeleterServiceMock
                .Setup(s => s.DeleteDeliveryByID(deliveryId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(deliveryId);

            // Assert
            _deliveriesDeleterServiceMock.Verify(s => s.DeleteDeliveryByID(deliveryId), Times.Once);
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_infoToastPartialView");
        }

        #endregion

        #region SetAsReceived

        [Fact]
        public async Task SetAsReceived_ServiceReturnsFalse_ReturnsErrorPartialView()
        {
            // Arrange
            var deliveryId = _fixture.Create<Guid>();

            _deliveriesUpdaterServiceMock
                .Setup(s => s.SetDeliveryAsReceived(deliveryId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.SetAsReceived(deliveryId);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
        }

        [Fact]
        public async Task SetAsReceived_ServiceReturnsTrue_ReturnsInfoPartialView()
        {
            // Arrange
            var deliveryId = _fixture.Create<Guid>();

            _deliveriesUpdaterServiceMock
                .Setup(s => s.SetDeliveryAsReceived(deliveryId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.SetAsReceived(deliveryId);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_infoToastPartialView");
        }

        #endregion
    }
}
