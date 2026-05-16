using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.ProductDTO;
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

        public ProductsController(IProductsGetterService productsGetterService, IProductsAdderService productsAdderService, IProductsUpdaterService productsUpdaterService, IProductsDeleterService productsDeleterService, ICategoriesGetterService categoriesGetterService)
        {
            _productsGetterService = productsGetterService;
            _productsAdderService = productsAdderService;
            _productsUpdaterService = productsUpdaterService;
            _productsDeleterService = productsDeleterService;
            _categoriesGetterService = categoriesGetterService;
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productsGetterService.GetAllProducts();
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
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(productAddRequest);
            }

            await _productsAdderService.AddProduct(productAddRequest);
            TempData["InfoMessage"] = $"Product has been added successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Authorize(Roles = "Administrator,Manager,Operator")]
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var product = await _productsGetterService.GetProductById(id);
            if (product is null)
            {
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

        [Authorize(Roles = "Administrator,Manager,Operator")]
        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(ProductUpdateRequest productUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(productUpdateRequest);
            }

            await _productsUpdaterService.UpdateProduct(productUpdateRequest);
            TempData["InfoMessage"] = $"Product has been updated successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _productsGetterService.GetProductById(id);
            if (product is null)
            {
                TempData["ErrorMessage"] = $"Product could not be found.";
                return RedirectToAction("Index", "Products");
            }
            var productDeleteRequest = product.ToProductDeleteRequest();
            return View(productDeleteRequest);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> Delete(ProductDeleteRequest productDeleteRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(productDeleteRequest);
            }

            await _productsDeleterService.DeleteProduct(productDeleteRequest.ProductID);
            TempData["InfoMessage"] = $"Product has been deleted successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditUnitPrice([FromBody] EditUnitPriceDTO model)
        {
            if (model.ProductID == Guid.Empty || model.NewUnitPrice is null)
            {
                ViewData["ErrorMessage"] = $"Unit price could not be changed.";
                return PartialView("_errorToastPartialView");
            }
            decimal unitPriceParsed = model.NewUnitPrice.ToDecimalSafe();
            if (unitPriceParsed <= 0)
            {
                ViewData["ErrorMessage"] = $"Unit price must be greater than zero.";
                return PartialView("_errorToastPartialView");
            }

            var matchingProduct = await _productsGetterService.GetProductById(model.ProductID);
            if (matchingProduct is null)
            {
                ViewData["ErrorMessage"] = $"Product could not be found";
                return PartialView("_errorToastPartialView");
            }
            matchingProduct.UnitPrice = unitPriceParsed;
            await _productsUpdaterService.UpdateProduct(matchingProduct.ToProductUpdateRequest());

            ViewData["InfoMessage"] = $"Unit price has been changed successfully.";
            return PartialView("_infoToastPartialView");
        }
    }
}
