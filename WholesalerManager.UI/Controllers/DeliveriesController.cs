using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.UI.Models;
using WholesalerManager.UI.ViewComponents;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class DeliveriesController : Controller
    {
        private readonly IDeliveriesGetterService _deliveriesGetterService;
        private readonly IDeliveriesAdderService _deliveriesAdderService;

        private readonly ISuppliersGetterService _suppliersGetterService;

        private readonly IProductsGetterService _productsGetterService;

        private readonly IDeliveryItemsAdderService _deliveryItemsAdderService;

        public DeliveriesController(IDeliveriesGetterService deliveriesGetterService, ISuppliersGetterService suppliersGetterService, IDeliveriesAdderService deliveriesAdderService, IProductsGetterService productsGetterService, IDeliveryItemsAdderService deliveryItemsAdderService)
        {
            _deliveriesGetterService = deliveriesGetterService;
            _deliveriesAdderService = deliveriesAdderService;
            _suppliersGetterService = suppliersGetterService;
            _productsGetterService = productsGetterService;
            _deliveryItemsAdderService = deliveryItemsAdderService;
        }

        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var deliveries = await _deliveriesGetterService.GetAllDeliveries();
            return View(deliveries);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var suppliers = await _suppliersGetterService.GetAllSuppliers();
            ViewBag.Suppliers = suppliers.Select(s => new SelectListItem()
            {
                Value = s.SupplierID.ToString(),
                Text = s.SupplierName
            });
            ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(RegisterDeliveryModel registerDeliveryViewModel)
        {
            if (registerDeliveryViewModel is null || registerDeliveryViewModel.Items is null)
            {
                TempData["ErrorMessage"] = $"Delivery could not be registered.";
                return RedirectToAction("Index", "Deliveries");
            }
            var delivery = await _deliveriesAdderService.AddDelivery(registerDeliveryViewModel.DeliveryAddRequest);

            registerDeliveryViewModel.Items.ForEach(i => i.DeliveryID = delivery.DeliveryID);
            await _deliveryItemsAdderService.AddMultipleDeliveryItems(registerDeliveryViewModel.Items);

            TempData["InfoMessage"] = $"Delivery has been registered successfully.";
            return RedirectToAction("Index", "Deliveries");
        }

        [Route("get-product/{index}")]
        [HttpGet]
        public IActionResult GetProduct(int index)
        {
            return ViewComponent("AddDeliveryItem", new { index = index });
        }
    }
}
