using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class CustomersController : Controller
    {
        private readonly ICustomersGetterService _customersGetterService;
        private readonly ICustomersAdderService _customersAdderService;
        private readonly ICustomersUpdaterService _customersUpdaterService;
        private readonly ICustomersDeleterService _customersDeleterService;

        public CustomersController(ICustomersGetterService customersGetterService, ICustomersAdderService customersAdderService, ICustomersUpdaterService customersUpdaterService, ICustomersDeleterService customersDeleterService)
        {
            _customersGetterService = customersGetterService;
            _customersAdderService = customersAdderService;
            _customersUpdaterService = customersUpdaterService;
            _customersDeleterService = customersDeleterService;
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
            bool isAdded = await _customersAdderService.AddCustomer(customerAddRequest);
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

        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var foundCustomer = await _customersGetterService.GetCustomerByID(id);
            if (foundCustomer is null)
            {
                TempData["ErrorMessage"] = "Customer could not be found.";
                return RedirectToAction("Index", "Customers");
            }

            return View(foundCustomer.ToCustomerUpdateRequest());
        }

        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(CustomerUpdateRequest customerUpdateRequest)
        {
            bool isUpdated = await _customersUpdaterService.UpdateCustomer(customerUpdateRequest);
            if (!isUpdated)
            {
                TempData["ErrorMessage"] = "Customer could not be updated.";
            }
            else
            {
                TempData["InfoMessage"] = "Customer has been successfully updated.";
            }
            return RedirectToAction("Index", "Customers");
        }

        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var foundCustomer = await _customersGetterService.GetCustomerByID(id);
            if (foundCustomer is null)
            {
                TempData["ErrorMessage"] = "Customer could not be found.";
                return RedirectToAction("Index", "Customers");
            }
            return View(foundCustomer.ToCustomerDeleteRequest());
        }

        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Delete(CustomerDeleteRequest customerDeleteRequest)
        {
            bool isDeleted = await _customersDeleterService.DeleteCustomer(customerDeleteRequest.CustomerID);
            if (!isDeleted)
            {
                TempData["ErrorMessage"] = "Customer could not be deleted.";
            }
            else
            {
                TempData["InfoMessage"] = "Customer has been successfully deleted.";
            }
            return RedirectToAction("Index", "Customers");
        }
    }
}
