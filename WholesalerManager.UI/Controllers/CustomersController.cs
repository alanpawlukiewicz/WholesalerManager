using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class CustomersController : Controller
    {
        private readonly ICustomersGetterService _customersGetterService;

        public CustomersController(ICustomersGetterService customersGetterService)
        {
            _customersGetterService = customersGetterService;
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _customersGetterService.GetAllCustomers();
            return View(customers);
        }
    }
}
