using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.UI.ViewModels;

namespace WholesalerManager.UI.ViewComponents
{
    public class AddDeliveryItemViewComponent : ViewComponent
    {
        private readonly IProductsGetterService _productsGetterService;

        public AddDeliveryItemViewComponent(IProductsGetterService productsGetterService)
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
