using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductsGetterService _productsGetterService;

        public ProductsController(IProductsGetterService productsGetterService)
        {
            _productsGetterService = productsGetterService;
        }

        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var products = await _productsGetterService.GetAllProducts();
            return View(products);
        }
    }
}
