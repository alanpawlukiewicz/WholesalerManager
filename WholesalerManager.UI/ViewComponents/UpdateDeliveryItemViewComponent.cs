using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.UI.ViewComponents
{
    public class UpdateDeliveryItemViewComponent : ViewComponent
    {
        private readonly IProductsGetterService _productsGetterService;

        public UpdateDeliveryItemViewComponent(IProductsGetterService productsGetterService)
        {
            _productsGetterService = productsGetterService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int index, Guid deliveryID)
        {
            ViewBag.Products = await _productsGetterService.GetAllProducts();
            ViewBag.Index = index;
            ViewBag.DeliveryID = deliveryID;
            return View();
        }
    }
}
