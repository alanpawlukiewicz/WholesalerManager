using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class DeliveriesController : Controller
    {
        private readonly IDeliveriesGetterService _deliveriesGetterService;
        private readonly IDeliveriesAdderService _deliveriesAdderService;
        private readonly ISuppliersGetterService _suppliersGetterService;

        public DeliveriesController(IDeliveriesGetterService deliveriesGetterService, ISuppliersGetterService suppliersGetterService, IDeliveriesAdderService deliveriesAdderService)
        {
            _deliveriesGetterService = deliveriesGetterService;
            _deliveriesAdderService = deliveriesAdderService;
            _suppliersGetterService = suppliersGetterService;
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
        public async Task<IActionResult> Create(DeliveryAddRequest deliveryAddRequest)
        {
            await _deliveriesAdderService.AddDelivery(deliveryAddRequest);
            TempData["InfoMessage"] = $"Delivery has been registered successfully.";

            return RedirectToAction("Index", "Deliveries");
        }
    }
}
