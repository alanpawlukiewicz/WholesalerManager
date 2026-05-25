using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.CategoryDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.UI.Controllers;

namespace WholesalerManager.ControllerTests
{


    public class ProductsControllerTests
    {
        private readonly Mock<IProductsGetterService> _productsGetterServiceMock;
        private readonly Mock<IProductsAdderService> _productsAdderServiceMock;
        private readonly Mock<IProductsUpdaterService> _productsUpdaterServiceMock;
        private readonly Mock<IProductsDeleterService> _productsDeleterServiceMock;
        private readonly Mock<ICategoriesGetterService> _categoriesGetterServiceMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly IFixture _fixture;
        private readonly ProductsController _sut;

        public ProductsControllerTests()
        {
            _productsGetterServiceMock = new Mock<IProductsGetterService>();
            _productsAdderServiceMock = new Mock<IProductsAdderService>();
            _productsUpdaterServiceMock = new Mock<IProductsUpdaterService>();
            _productsDeleterServiceMock = new Mock<IProductsDeleterService>();
            _categoriesGetterServiceMock = new Mock<ICategoriesGetterService>();
            _loggerMock = new Mock<ILogger<ProductsController>>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new ProductsController(
                _productsGetterServiceMock.Object,
                _productsAdderServiceMock.Object,
                _productsUpdaterServiceMock.Object,
                _productsDeleterServiceMock.Object,
                _categoriesGetterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region Index

        [Fact]
        public async Task Index_NoFilterNoSort_CallsGetAllProductsAndReturnsView()
        {
            // Arrange – no filter or sort parameters provided
            var products = _fixture.CreateMany<ProductResponse>(3).ToList();

            _productsGetterServiceMock
                .Setup(s => s.GetAllProducts())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.Index(null, null, null, null);

            // Assert – GetAllProducts called, view returned with correct model
            _productsGetterServiceMock.Verify(s => s.GetAllProducts(), Times.Once);
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task Index_WithFilterAndPropertyName_CallsGetFilteredProducts()
        {
            // Arrange – both filter and propertyName provided
            var products = _fixture.CreateMany<ProductResponse>(2).ToList();

            _productsGetterServiceMock
                .Setup(s => s.GetFilteredProducts(
                    nameof(ProductResponse.ProductName), "Laptop", false))
                .ReturnsAsync(products);

            // Act
            var result = await _sut.Index(nameof(ProductResponse.ProductName), "Laptop", false, null);

            // Assert – GetFilteredProducts called with correct arguments
            _productsGetterServiceMock.Verify(s => s.GetFilteredProducts(
                nameof(ProductResponse.ProductName), "Laptop", false), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task Index_WithSortOrderAndPropertyName_CallsGetSortedProducts()
        {
            // Arrange – sortOrder and propertyName provided, no filter
            var products = _fixture.CreateMany<ProductResponse>(3).ToList();

            _productsGetterServiceMock
                .Setup(s => s.GetSortedProducts(
                    nameof(ProductResponse.ProductName), SortOrderOptions.ASC))
                .ReturnsAsync(products);

            // Act
            var result = await _sut.Index(nameof(ProductResponse.ProductName), null, null, SortOrderOptions.ASC);

            // Assert
            _productsGetterServiceMock.Verify(s => s.GetSortedProducts(
                nameof(ProductResponse.ProductName), SortOrderOptions.ASC), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task Index_SetsViewBagFieldNames()
        {
            // Arrange
            _productsGetterServiceMock
                .Setup(s => s.GetAllProducts())
                .ReturnsAsync(new List<ProductResponse>());

            // Act
            await _sut.Index(null, null, null, null);

            // Assert – ViewBag.FieldNames contains the expected property names
            var fieldNames = _sut.ViewBag.FieldNames as List<string>;
            fieldNames.Should().NotBeNull();
            fieldNames.Should().Contain(nameof(ProductResponse.ProductName));
            fieldNames.Should().Contain(nameof(ProductResponse.SKU));
            fieldNames.Should().Contain(nameof(ProductResponse.CategoryName));
            fieldNames.Should().Contain(nameof(ProductResponse.ProductDescription));
        }

        #endregion

        #region Create GET

        [Fact]
        public async Task Create_Get_ReturnsViewWithCategories()
        {
            // Arrange – categories loaded into ViewBag for the create form
            var categories = _fixture.CreateMany<CategoryResponse>(3).ToList();

            _categoriesGetterServiceMock
                .Setup(s => s.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _sut.Create();

            // Assert – view returned and ViewBag.Categories populated
            result.Should().BeOfType<ViewResult>();
            var viewBagCategories = _sut.ViewBag.Categories as IEnumerable<SelectListItem>;
            viewBagCategories.Should().NotBeNull();
            viewBagCategories!.Should().HaveCount(3);
        }

        #endregion

        #region Create POST

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithRequest()
        {
            // Arrange – simulate invalid model state
            var request = _fixture.Create<ProductAddRequest>();
            _sut.ModelState.AddModelError("ProductName", "Product name is required.");

            // Act
            var result = await _sut.Create(request);

            // Assert – view returned with the original request, adder service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(request);
            _productsAdderServiceMock.Verify(s => s.AddProduct(It.IsAny<ProductAddRequest>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_ValidRequest_AddsProductAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();

            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = _fixture.Build<ProductAddRequest>()
                .With(r => r.UnitPrice, "99.99")
                .With(r => r.CategoryID, Guid.NewGuid())
                .Create();

            _productsAdderServiceMock
                .Setup(s => s.AddProduct(request))
                .ReturnsAsync(_fixture.Create<ProductResponse>());

            // Act
            var result = await _sut.Create(request);

            // Assert – product added and redirected to Index
            _productsAdderServiceMock.Verify(s => s.AddProduct(request), Times.Once);
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Products");
        }

        #endregion

        #region Update GET

        [Fact]
        public async Task Update_Get_NonExistentProduct_RedirectsToIndex()
        {
            // Arrange – product not found
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();

            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var productId = _fixture.Create<Guid>();

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(productId))
                .ReturnsAsync((ProductResponse?)null);

            // Act
            var result = await _sut.Update(productId);

            // Assert – redirected to Index when product is not found
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Products");
        }

        [Fact]
        public async Task Update_Get_ExistingProduct_ReturnsViewWithUpdateRequest()
        {
            // Arrange
            var product = _fixture.Create<ProductResponse>();
            var categories = _fixture.CreateMany<CategoryResponse>(3).ToList();

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(product.ProductID))
                .ReturnsAsync(product);

            _categoriesGetterServiceMock
                .Setup(s => s.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _sut.Update(product.ProductID);

            // Assert – view returned with ProductUpdateRequest mapped from the product
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<ProductUpdateRequest>().Subject;
            model.ProductID.Should().Be(product.ProductID);
        }

        #endregion

        #region Update POST

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithRequest()
        {
            // Arrange
            var request = _fixture.Create<ProductUpdateRequest>();
            _sut.ModelState.AddModelError("ProductName", "Product name is required.");

            // Act
            var result = await _sut.Update(request);

            // Assert – view returned, updater service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(request);
            _productsUpdaterServiceMock.Verify(s => s.UpdateProduct(It.IsAny<ProductUpdateRequest>()), Times.Never);
        }

        [Fact]
        public async Task Update_Post_ValidRequest_UpdatesProductAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();

            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = _fixture.Build<ProductUpdateRequest>()
                .With(r => r.UnitPrice, "99.99")
                .With(r => r.CategoryID, Guid.NewGuid())
                .Create();

            _productsUpdaterServiceMock
                .Setup(s => s.UpdateProduct(request))
                .ReturnsAsync(_fixture.Create<ProductResponse>());

            // Act
            var result = await _sut.Update(request);

            // Assert
            _productsUpdaterServiceMock.Verify(s => s.UpdateProduct(request), Times.Once);
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Products");
        }

        #endregion

        #region Delete GET

        [Fact]
        public async Task Delete_Get_NonExistentProduct_RedirectsToIndex()
        {
            // Arrange – product not found
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();

            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var productId = _fixture.Create<Guid>();

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(productId))
                .ReturnsAsync((ProductResponse?)null);

            // Act
            var result = await _sut.Delete(productId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Products");
            _sut.TempData["ErrorMessage"].Should().Be("Product could not be found.");
        }

        [Fact]
        public async Task Delete_Get_ExistingProduct_ReturnsViewWithDeleteRequest()
        {
            // Arrange
            var product = _fixture.Create<ProductResponse>();

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(product.ProductID))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.Delete(product.ProductID);

            // Assert – view returned with ProductDeleteRequest mapped from the product
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<ProductDeleteRequest>();
        }

        #endregion

        #region Delete POST

        [Fact]
        public async Task Delete_Post_InvalidModelState_ReturnsViewWithRequest()
        {
            // Arrange
            var request = _fixture.Create<ProductDeleteRequest>();
            _sut.ModelState.AddModelError("ProductID", "Product ID is required.");

            // Act
            var result = await _sut.Delete(request);

            // Assert – view returned, deleter service never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(request);
            _productsDeleterServiceMock.Verify(s => s.DeleteProduct(It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_Post_ValidRequest_DeletesProductAndRedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();

            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            var request = _fixture.Create<ProductDeleteRequest>();

            _productsDeleterServiceMock
                .Setup(s => s.DeleteProduct(request.ProductID))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(request);

            // Assert
            _productsDeleterServiceMock.Verify(s => s.DeleteProduct(request.ProductID), Times.Once);
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Products");
        }

        #endregion

        #region EditUnitPrice

        [Fact]
        public async Task EditUnitPrice_ZeroOrNegativePrice_ReturnsErrorPartialView()
        {
            // Arrange – price of 0 or below should return error partial view
            var model = new EditUnitPriceDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewUnitPrice = "0"
            };

            // Act
            var result = await _sut.EditUnitPrice(model);

            // Assert – error toast returned, updater service never called
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
            _productsUpdaterServiceMock.Verify(s => s.UpdateUnitPrice(It.IsAny<EditUnitPriceDTO>()), Times.Never);
        }

        [Fact]
        public async Task EditUnitPrice_ServiceReturnsFalse_ReturnsErrorPartialView()
        {
            // Arrange – service signals update failure
            var model = new EditUnitPriceDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewUnitPrice = "49.99"
            };

            _productsUpdaterServiceMock
                .Setup(s => s.UpdateUnitPrice(model))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.EditUnitPrice(model);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
        }

        [Fact]
        public async Task EditUnitPrice_ValidPrice_ReturnsInfoPartialView()
        {
            // Arrange – valid price and successful update
            var model = new EditUnitPriceDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewUnitPrice = "149.99"
            };

            _productsUpdaterServiceMock
                .Setup(s => s.UpdateUnitPrice(model))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.EditUnitPrice(model);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_infoToastPartialView");
        }

        #endregion

        #region EditStockQuantity

        [Fact]
        public async Task EditStockQuantity_NegativeQuantity_ReturnsErrorPartialView()
        {
            // Arrange – negative stock quantity should return error partial view
            var model = new EditStockQuantityDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewStockQuantity = -1
            };

            // Act
            var result = await _sut.EditStockQuantity(model);

            // Assert – error toast returned, updater service never called
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
            _productsUpdaterServiceMock.Verify(s => s.UpdateStockQuantity(It.IsAny<EditStockQuantityDTO>()), Times.Never);
        }

        [Fact]
        public async Task EditStockQuantity_ServiceReturnsFalse_ReturnsErrorPartialView()
        {
            // Arrange – service signals update failure
            var model = new EditStockQuantityDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewStockQuantity = 10
            };

            _productsUpdaterServiceMock
                .Setup(s => s.UpdateStockQuantity(model))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.EditStockQuantity(model);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_errorToastPartialView");
        }

        [Fact]
        public async Task EditStockQuantity_ValidQuantity_ReturnsInfoPartialView()
        {
            // Arrange – valid quantity and successful update
            var model = new EditStockQuantityDTO
            {
                ProductID = _fixture.Create<Guid>(),
                NewStockQuantity = 50
            };

            _productsUpdaterServiceMock
                .Setup(s => s.UpdateStockQuantity(model))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.EditStockQuantity(model);

            // Assert
            var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialResult.ViewName.Should().Be("_infoToastPartialView");
        }

        #endregion
    }
}
