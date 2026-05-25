using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.Areas.Manager.Controllers
{
    [Authorize(Roles = "Manager")]
    [Area("Manager")]
    [Route("[area]/[controller]/[action]")]
    public class ProductsController : Controller
    {
        private readonly IProductsGetterService _productsGetterService;

        public ProductsController(IProductsGetterService productsGetterService)
        {
            _productsGetterService = productsGetterService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductsNeedingReorder()
        {
            var products = await _productsGetterService.GetProductsNeedingReorder();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ProductsNeedingReorderPDF()
        {
            var products = await _productsGetterService.GetProductsNeedingReorder();
            ViewBag.CurrentDateTime = DateTime.Now.ToString("g");
            return new ViewAsPdf("ProductsNeedingReorderPDF", products, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 20, 20, 20),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
    }
}
