using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.Areas.Manager.Controllers
{
    [Authorize(Roles = "Manager")]
    [Area("Manager")]
    [Route("[area]/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly IProductsGetterService _productsGetterService;

        public HomeController(IProductsGetterService productsGetterService)
        {
            _productsGetterService = productsGetterService;
        }

        [HttpGet("/Manager")]
        public async Task<IActionResult> Index()
        {
            ViewBag.NumberofProductsNeedingReorder = await _productsGetterService.GetNumberOfProductsNeedingReorder();
            return View();
        }
    }
}
