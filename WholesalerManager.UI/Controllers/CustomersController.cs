using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class CustomersController : Controller
    {
        private readonly ICustomersGetterService _customersGetterService;
        private readonly ICustomerAdderService _customerAdderService;

        public CustomersController(ICustomersGetterService customersGetterService, ICustomerAdderService customerAdderService)
        {
            _customersGetterService = customersGetterService;
            _customerAdderService = customerAdderService;
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _customersGetterService.GetAllCustomers();
            return View(customers);
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(CustomerAddRequest customerAddRequest)
        {
            bool isAdded = await _customerAdderService.AddCustomer(customerAddRequest);
            if (!isAdded) 
            {
                TempData["ErrorMessage"] = "Customer could not be registered.";
            }
            else
            {
                TempData["InfoMessage"] = "Customer has been successfully registered.";
            }
            return RedirectToAction("Index", "Customers");
        }
    }
}
