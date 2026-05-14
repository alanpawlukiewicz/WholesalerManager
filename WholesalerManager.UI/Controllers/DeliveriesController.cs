using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
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

        private readonly IDeliveryItemsAdderService _deliveryItemsAdderService;
        private readonly IDeliveryItemsGetterService _deliveryItemsGetterService;
        private readonly IDeliveryItemsUpdaterService _deliveryItemsUpdaterService;

        private readonly ISuppliersGetterService _suppliersGetterService;

        private readonly IProductsGetterService _productsGetterService;

        public DeliveriesController(IDeliveriesGetterService deliveriesGetterService, 
            IDeliveriesAdderService deliveriesAdderService, 
            IDeliveriesUpdaterService deliveriesUpdaterService, 
            IDeliveriesDeleterService deliveriesDeleterService, 
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

            _deliveryItemsAdderService = deliveryItemsAdderService;
            _deliveryItemsGetterService = deliveryItemsGetterService;
            _deliveryItemsUpdaterService = deliveryItemsUpdaterService;

            _suppliersGetterService = suppliersGetterService;

            _productsGetterService = productsGetterService;
        }

        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var deliveries = await _deliveriesGetterService.GetAllDeliveries();
            var items = await _deliveryItemsGetterService.GetAllDeliveryItems();
            List<DeliveryWithProductsViewModel> model = new List<DeliveryWithProductsViewModel>();
            foreach(var delivery in deliveries)
            {
                DeliveryWithProductsViewModel c = new DeliveryWithProductsViewModel()
                {
                    Delivery = delivery,
                    Items = items.Where(i => i.DeliveryID == delivery.DeliveryID).ToList()
                };
                model.Add(c);
            }
            return View(model);
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
        public async Task<IActionResult> Create(RegisterDeliveryViewModel registerDeliveryViewModel)
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
                Items = deliveryItems?.Select(i => i?.ToDeliveryItemUpdateRequest()).ToList()
            };

            ViewBag.Products = await _productsGetterService.GetAllProducts();

            return View(updateDeliveryWithProductsModel);
        }

        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateDeliveryWithProductsViewModel updateDeliveryWithProductsModel)
        {
            await _deliveriesUpdaterService.UpdateDelivery(updateDeliveryWithProductsModel.Delivery);
            await _deliveryItemsUpdaterService.UpdateMultipleDeliveryItems(updateDeliveryWithProductsModel.Items);
            TempData["InfoMessage"] = "Delivery data have been updated successfully.";

            return RedirectToAction("Index", "Deliveries");
        }

        [Route("[action]/{deliveryID}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid deliveryID)
        {
            if (deliveryID == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Delivery data could not be deleted.";
                return PartialView("_errorToastPartialView");
            }

            await _deliveryDeleterService.DeleteDeliveryByID(deliveryID);

            ViewData["InfoMessage"] = "Delivery data have been deleted successfully.";
            return PartialView("_infoToastPartialView");
        }



        [Route("get-product/{index}")]
        [HttpGet]
        public IActionResult GetProduct(int index)
        {
            return ViewComponent("AddDeliveryItem", new { index = index });
        }

        [Route("Update/get-update-product/{index}/{deliveryID}")]
        [HttpGet]
        public IActionResult GetUpdateProduct(int index, Guid deliveryID)
        {
            return ViewComponent("UpdateDeliveryItem", new { index = index, deliveryID = deliveryID });
        }
    }
}
