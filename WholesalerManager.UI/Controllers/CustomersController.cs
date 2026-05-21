using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.UI.Helpers;

namespace WholesalerManager.UI.Controllers
{
    [Authorize(Roles = "Administrator,Manager,Sales")]
    [Route("[controller]/[action]")]
    public class CustomersController : Controller
    {
        private readonly ICustomersGetterService _customersGetterService;
        private readonly ICustomersAdderService _customersAdderService;
        private readonly ICustomersUpdaterService _customersUpdaterService;
        private readonly ICustomersDeleterService _customersDeleterService;

        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomersGetterService customersGetterService, ICustomersAdderService customersAdderService, ICustomersUpdaterService customersUpdaterService, ICustomersDeleterService customersDeleterService, ILogger<CustomersController> logger)
        {
            _customersGetterService = customersGetterService;
            _customersAdderService = customersAdderService;
            _customersUpdaterService = customersUpdaterService;
            _customersDeleterService = customersDeleterService;

            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<CustomerResponse> customers;

            if (filter is not null && propertyName is not null)
            {
                customers = await _customersGetterService.GetFilteredCustomers(propertyName, filter, ignoreCase ?? false);
            }
            else if (sortOrder is not null && propertyName is not null)
            {
                customers = await _customersGetterService.GetSortedCustomers(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                customers = await _customersGetterService.GetAllCustomers();
            }

            ViewBag.FieldNames = typeof(CustomerResponse).GetProperties().Select(p => p.Name).Where(p => p != nameof(CustomerResponse.CustomerID)).ToList();

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
                _logger.LogWarning("Customer creation failed due to invalid model state. Errors: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(customerAddRequest);
            }

            bool isAdded = await _customersAdderService.AddCustomer(customerAddRequest);
            if (!isAdded)
            {
                _logger.LogError("Failed to add customer.");
                TempData["ErrorMessage"] = "Customer could not be registered.";
            }
            else
            {
                _logger.LogInformation("Customer has been successfully registered.");
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
                _logger.LogWarning("Customer with ID {CustomerID} could not be found for update.", id);
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
                _logger.LogWarning("Customer update failed due to invalid model state. Errors: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(customerUpdateRequest);
            }

            bool isUpdated = await _customersUpdaterService.UpdateCustomer(customerUpdateRequest);
            if (!isUpdated)
            {
                _logger.LogError("Failed to update customer with ID {CustomerID}.", customerUpdateRequest.CustomerID);
                TempData["ErrorMessage"] = "Customer could not be updated.";
            }
            else
            {
                _logger.LogInformation("Customer with ID {CustomerID} has been successfully updated.", customerUpdateRequest.CustomerID);
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
                _logger.LogError("Customer with ID {CustomerID} could not be found for deletion.", id);
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
                _logger.LogWarning("Customer deletion failed due to invalid model state. Errors: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(customerDeleteRequest);
            }

            bool isDeleted = await _customersDeleterService.DeleteCustomer(customerDeleteRequest);
            if (!isDeleted)
            {
                _logger.LogError("Failed to delete customer with ID {CustomerID}.", customerDeleteRequest.CustomerID);
                TempData["ErrorMessage"] = "Customer could not be deleted.";
            }
            else
            {
                _logger.LogInformation("Customer with ID {CustomerID} has been successfully deleted.", customerDeleteRequest.CustomerID);
                TempData["InfoMessage"] = "Customer has been successfully deleted.";
            }
            return RedirectToAction("Index", "Customers");
        }
    }
}
