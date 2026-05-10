using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
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

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productsGetterService.GetAllProducts();
            return View(products);
        }

        [Route("[action]")]
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

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductAddRequest productAddRequest)
        {
            await _productsAdderService.AddProduct(productAddRequest);
            TempData["InfoMessage"] = $"Product has been added successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Route("[action]/{id}")]
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

        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(ProductUpdateRequest productUpdateRequest)
        {
            await _productsUpdaterService.UpdateProduct(productUpdateRequest);
            TempData["InfoMessage"] = $"Product has been updated successfully.";

            return RedirectToAction("Index", "Products");
        }

        [Route("[action]/{id}")]
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

        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Delete(ProductDeleteRequest productDeleteRequest)
        {
            await _productsDeleterService.DeleteProduct(productDeleteRequest.ProductID);
            TempData["InfoMessage"] = $"Product has been deleted successfully.";

            return RedirectToAction("Index", "Products");
        }
    }
}
