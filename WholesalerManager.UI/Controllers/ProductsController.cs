using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.UI.ViewModels;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class ProductsController : Controller
    {
        private readonly IProductsGetterService _productsGetterService;
        private readonly IProductsAdderService _productsAdderService;
        private readonly IProductsUpdaterService _productsUpdaterService;
        private readonly IProductsDeleterService _productsDeleterService;

        private readonly ICategoriesGetterService _categoriesGetterService;

        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductsGetterService productsGetterService, IProductsAdderService productsAdderService, IProductsUpdaterService productsUpdaterService, IProductsDeleterService productsDeleterService, ICategoriesGetterService categoriesGetterService, ILogger<ProductsController> logger)
        {
            _productsGetterService = productsGetterService;
            _productsAdderService = productsAdderService;
            _productsUpdaterService = productsUpdaterService;
            _productsDeleterService = productsDeleterService;
            _categoriesGetterService = categoriesGetterService;
            _logger = logger;
        }

        [Authorize(Roles = "Administrator,Manager,Sales,Operator")]
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<ProductResponse> products;

            if (filter is not null && propertyName is not null)
            {
                products = await _productsGetterService.GetFilteredProducts(propertyName, filter, ignoreCase ?? true);
            }

            else if (sortOrder is not null && propertyName is not null)
            {
                products = await _productsGetterService.GetSortedProducts(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                products = await _productsGetterService.GetAllProducts();
            }

            ViewBag.FieldNames = new List<string>() { nameof(ProductResponse.ProductName), nameof(ProductResponse.SKU), nameof(ProductResponse.CategoryName), nameof(ProductResponse.ProductDescription) };

            return View(products);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoriesGetterService.GetAllCategories();
            ViewBag.Categories = categories.Select(c => new SelectListItem()
            {
                Value = c.CategoryID.ToString(),
                Text = c.CategoryName
            });
            return View();
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductAddRequest productAddRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(productAddRequest);
            }

            await _productsAdderService.AddProduct(productAddRequest);
            _logger.LogInformation("Product with name {ProductName} has been added successfully.", productAddRequest.ProductName);
            TempData["InfoMessage"] = $"Product has been added successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Update(Guid id)
        {
            var product = await _productsGetterService.GetProductById(id);
            if (product is null)
            {
                _logger.LogWarning("Product with ID {ProductID} could not be found.", id);
                TempData["ErrorMessage"] = $"Product could not be found.";
                return RedirectToAction("Index", "Products");
            }

            var productUpdateRequest = product.ToProductUpdateRequest();
            var categories = await _categoriesGetterService.GetAllCategories();
            ViewBag.Categories = categories.Select(c => new SelectListItem()
            {
                Value = c.CategoryID.ToString(),
                Text = c.CategoryName
            });

            return View(productUpdateRequest);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(ProductUpdateRequest productUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {Errors}", ModelState.GetErrorMessages());

                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(productUpdateRequest);
            }

            await _productsUpdaterService.UpdateProduct(productUpdateRequest);
            _logger.LogInformation("Product with ID {ProductID} has been updated successfully.", productUpdateRequest.ProductID);
            TempData["InfoMessage"] = $"Product has been updated successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _productsGetterService.GetProductById(id);
            if (product is null)
            {
                _logger.LogWarning("Product with ID {ProductID} could not be found.", id);
                TempData["ErrorMessage"] = $"Product could not be found.";
                return RedirectToAction("Index", "Products");
            }
            var productDeleteRequest = product.ToProductDeleteRequest();
            return View(productDeleteRequest);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(ProductDeleteRequest productDeleteRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {Errors}", ModelState.GetErrorMessages());


                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(productDeleteRequest);
            }

            await _productsDeleterService.DeleteProduct(productDeleteRequest.ProductID);
            _logger.LogInformation("Product with ID {ProductID} has been deleted successfully.", productDeleteRequest.ProductID);
            TempData["InfoMessage"] = $"Product has been deleted successfully.";

            return RedirectToAction("Index", "Products");
        }

        [HttpGet]
        public async Task<IActionResult> ProductsPDF()
        {
            var products = await _productsGetterService.GetAllProducts();
            ViewBag.CurrentDateTime = DateTime.Now.ToString("g");
            return new ViewAsPdf("ProductsPDF", products, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 20, 20, 20),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Authorize(Roles = "Administrator,Sales")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditUnitPrice([FromBody] EditUnitPriceDTO model)
        {
            if (model.NewUnitPrice.ToDecimalSafe() <= 0)
            {
                ViewData["ErrorMessage"] = $"Unit price must be greater than zero.";
                return PartialView("_errorToastPartialView");

            }

            bool result = await _productsUpdaterService.UpdateUnitPrice(model);

            if (!result)
            {
                _logger.LogError("Failed to update unit price for product with ID {ProductID}", model.ProductID);

                ViewData["ErrorMessage"] = $"Unit price could not be changed.";
                return PartialView("_errorToastPartialView");
            }

            _logger.LogInformation("Unit price for product with ID {ProductID} has been updated successfully.", model.ProductID);
            ViewData["InfoMessage"] = $"Unit price has been changed successfully.";
            return PartialView("_infoToastPartialView");
        }

        [Authorize(Roles = "Administrator,Operator")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditStockQuantity([FromBody] EditStockQuantityDTO model)
        {
            if (model.NewStockQuantity < 0)
            {
                ViewData["ErrorMessage"] = $"Stock quantity must be a positive number.";
                return PartialView("_errorToastPartialView");
            }

            bool result = await _productsUpdaterService.UpdateStockQuantity(model);

            if (!result)
            {
                _logger.LogError("Failed to update stock quantity for product with ID {ProductID}", model.ProductID);
                ViewData["ErrorMessage"] = $"Stock quantity could not be changed.";
                return PartialView("_errorToastPartialView");
            }

            _logger.LogInformation("Stock quantity for product with ID {ProductID} has been updated successfully.", model.ProductID);
            ViewData["InfoMessage"] = $"Stock quantity has been changed successfully.";
            return PartialView("_infoToastPartialView");
        }
    }
}
