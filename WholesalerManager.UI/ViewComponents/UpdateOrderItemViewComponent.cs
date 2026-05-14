using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.ViewComponents
{
    public class UpdateOrderItemViewComponent : ViewComponent
    {
        private readonly IProductsGetterService _productsGetterService;

        public UpdateOrderItemViewComponent(IProductsGetterService productsGetterService)
        {
            _productsGetterService = productsGetterService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int index, Guid orderID)
        {
            ViewBag.Products = await _productsGetterService.GetAllProducts();
            ViewBag.Index = index;
            ViewBag.OrderID = orderID;
            return View();
        }
    }
}
