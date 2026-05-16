using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _customersGetterService.GetAllCustomers();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerAddRequest customerAddRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
                return View(customerAddRequest);
            }

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

        [Route("{id}")]
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

        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(CustomerUpdateRequest customerUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
                return View(customerUpdateRequest);
            }

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

        [Route("{id}")]
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

        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> Delete(CustomerDeleteRequest customerDeleteRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
                return View(customerDeleteRequest);
            }

            bool isDeleted = await _customersDeleterService.DeleteCustomer(customerDeleteRequest);
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
