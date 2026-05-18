using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;
using WholesalerManager.Core.Services.ProductServices;
using WholesalerManager.Core.Services.SupplierServices;
using WholesalerManager.UI.ViewComponents;
using WholesalerManager.UI.ViewModels;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class DeliveriesController : Controller
    {
        private readonly IDeliveriesGetterService _deliveriesGetterService;
        private readonly IDeliveriesAdderService _deliveriesAdderService;
        private readonly IDeliveriesUpdaterService _deliveriesUpdaterService;
        private readonly IDeliveriesDeleterService _deliveryDeleterService;
        private readonly IDeliveryRegistrationService _deliveryRegistrationService;
        private readonly IDeliveryUpdateControllerService _deliveryUpdateControllerService;

        private readonly IDeliveryItemsAdderService _deliveryItemsAdderService;
        private readonly IDeliveryItemsGetterService _deliveryItemsGetterService;
        private readonly IDeliveryItemsUpdaterService _deliveryItemsUpdaterService;

        private readonly ISuppliersGetterService _suppliersGetterService;

        private readonly IProductsGetterService _productsGetterService;

        public DeliveriesController(IDeliveriesGetterService deliveriesGetterService, 
            IDeliveriesAdderService deliveriesAdderService, 
            IDeliveriesUpdaterService deliveriesUpdaterService, 
            IDeliveriesDeleterService deliveriesDeleterService, 
            IDeliveryRegistrationService deliveryRegistrationService,
            IDeliveryUpdateControllerService deliveryUpdateControllerService,

            IDeliveryItemsAdderService deliveryItemsAdderService, 
            IDeliveryItemsGetterService deliveryItemsGetterService, 
            IDeliveryItemsUpdaterService deliveryItemsUpdaterService, 
            ISuppliersGetterService suppliersGetterService, 
            IProductsGetterService productsGetterService)
        {
            _deliveriesGetterService = deliveriesGetterService;
            _deliveriesAdderService = deliveriesAdderService;
            _deliveryDeleterService = deliveriesDeleterService;
            _deliveriesUpdaterService = deliveriesUpdaterService;

            _deliveryRegistrationService = deliveryRegistrationService;
            _deliveryUpdateControllerService = deliveryUpdateControllerService;

            _deliveryItemsAdderService = deliveryItemsAdderService;
            _deliveryItemsGetterService = deliveryItemsGetterService;
            _deliveryItemsUpdaterService = deliveryItemsUpdaterService;

            _suppliersGetterService = suppliersGetterService;

            _productsGetterService = productsGetterService;
        }

        [Authorize(Roles = "Administrator,Manager,Operator")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<DeliveryResponse> deliveries;

            if (filter is not null && propertyName is not null)
            {
                deliveries = await _deliveriesGetterService.GetFilteredDeliveries(propertyName, filter, ignoreCase ?? true);
            }

            else if (sortOrder is not null && propertyName is not null)
            {
                deliveries = await _deliveriesGetterService.GetSortedDeliveries(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                deliveries = await _deliveriesGetterService.GetAllDeliveries();
            }

            var items = await _deliveryItemsGetterService.GetAllDeliveryItems();

            List<DeliveryWithProductsViewModel> model = new List<DeliveryWithProductsViewModel>();
            foreach (var delivery in deliveries)
            {
                DeliveryWithProductsViewModel c = new DeliveryWithProductsViewModel()
                {
                    Delivery = delivery,
                    Items = items.Where(i => i.DeliveryID == delivery.DeliveryID).ToList()
                };
                model.Add(c);
            }

            ViewBag.FieldNames = new List<string>() { nameof(DeliveryResponse.SupplierName), nameof(DeliveryResponse.Status), nameof(DeliveryResponse.OrderDate) };
            return View(model);
        }

        [Authorize(Roles = "Administrator,Manager")]
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

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(RegisterDeliveryViewModel registerDeliveryViewModel)
        {
            if (!ModelState.IsValid)
            {
                var suppliers = await _suppliersGetterService.GetAllSuppliers();
                ViewBag.Suppliers = suppliers.Select(s => new SelectListItem()
                {
                    Value = s.SupplierID.ToString(),
                    Text = s.SupplierName
                });
                ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");

                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(registerDeliveryViewModel);
            }

            if (registerDeliveryViewModel is null || registerDeliveryViewModel.Items is null)
            {
                TempData["ErrorMessage"] = $"Delivery could not be registered.";
                return RedirectToAction("Index", "Deliveries");
            }

            await _deliveryRegistrationService.RegisterFullDelivery(registerDeliveryViewModel.DeliveryAddRequest, registerDeliveryViewModel.Items);

            TempData["InfoMessage"] = $"Delivery has been registered successfully.";
            return RedirectToAction("Index", "Deliveries");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var foundDelivery = await _deliveriesGetterService.GetDeliveryById(id);
            var deliveryUpdateRequest = foundDelivery?.ToDeliveryUpdateRequest();
            var deliveryItems = await _deliveryItemsGetterService.GetAllDeliveryItemsFromDelivery(id);

            var suppliers = await _suppliersGetterService.GetAllSuppliers();
            ViewBag.Suppliers = suppliers.Select(s => new SelectListItem()
            {
                Value = s.SupplierID.ToString(),
                Text = s.SupplierName
            });

            var updateDeliveryWithProductsModel = new UpdateDeliveryWithProductsViewModel()
            {
                Delivery = deliveryUpdateRequest,
                Items = deliveryItems?.Select(i => i.ToDeliveryItemUpdateRequest()).ToList()
            };

            ViewBag.Products = await _productsGetterService.GetAllProducts();

            return View(updateDeliveryWithProductsModel);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateDeliveryWithProductsViewModel updateDeliveryWithProductsModel)
        {
            if (!ModelState.IsValid)
            {
                var suppliers = await _suppliersGetterService.GetAllSuppliers();
                ViewBag.Suppliers = suppliers.Select(s => new SelectListItem()
                {
                    Value = s.SupplierID.ToString(),
                    Text = s.SupplierName
                });

                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(updateDeliveryWithProductsModel);
            }

            // TODO: Make it into single transaction

            await _deliveryUpdateControllerService.UpdateFullDelivery(updateDeliveryWithProductsModel.Delivery, updateDeliveryWithProductsModel.Items);
            TempData["InfoMessage"] = "Delivery data have been updated successfully.";

            return RedirectToAction("Index", "Deliveries");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]/{deliveryID}")]
        [HttpDelete]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(Guid deliveryID)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(deliveryID);
            }

            if (deliveryID == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Delivery data could not be deleted.";
                return PartialView("_errorToastPartialView");
            }

            await _deliveryDeleterService.DeleteDeliveryByID(deliveryID);

            ViewData["InfoMessage"] = "Delivery data have been deleted successfully.";
            return PartialView("_infoToastPartialView");
        }


        [Authorize(Roles = "Administrator,Manager")]
        [Route("get-product/{index}")]
        [HttpGet]
        public IActionResult GetProduct(int index)
        {
            return ViewComponent("AddDeliveryItem", new { index = index });
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("Update/get-update-product/{index}/{deliveryID}")]
        [HttpGet]
        public IActionResult GetUpdateProduct(int index, Guid deliveryID)
        {
            return ViewComponent("UpdateDeliveryItem", new { index = index, deliveryID = deliveryID });
        }

        [Authorize(Roles = "Administrator,Operator")]
        [Route("[action]")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SetAsReceived([FromBody] Guid deliveryID)
        {
            var result = await _deliveriesUpdaterService.SetDeliveryAsReceived(deliveryID);
            if (!result)
            {
                ViewData["ErrorMessage"] = "Delivery data could not be updated.";
                return PartialView("_errorToastPartialView");
            }

            ViewData["InfoMessage"] = "Delivery data have been updated successfully.";
            return PartialView("_infoToastPartialView");
        }
    }
}
