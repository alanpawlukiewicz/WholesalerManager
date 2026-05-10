using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class DeliveriesController : Controller
    {
        private readonly IDeliveriesGetterService _deliveriesGetterService;

        public DeliveriesController(IDeliveriesGetterService deliveriesGetterService    )
        {
            _deliveriesGetterService = deliveriesGetterService;
        }

        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var deliveries = await _deliveriesGetterService.GetAllDeliveries();
            return View(deliveries);
        }
    }
}
