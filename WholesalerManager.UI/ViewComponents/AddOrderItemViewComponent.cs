using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.ViewComponents
{
    public class AddOrderItemViewComponent : ViewComponent
    {
        private readonly IProductsGetterService _productsGetterService;

        public AddOrderItemViewComponent(IProductsGetterService productsGetterService)
        {
            _productsGetterService = productsGetterService;
        }



        public async Task<IViewComponentResult> InvokeAsync(int index)
        {
            ViewBag.Products = await _productsGetterService.GetAllProducts();
            ViewBag.Index = index;
            return View();
        }
    }
}
