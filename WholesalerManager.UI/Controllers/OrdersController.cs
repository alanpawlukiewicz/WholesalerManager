using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.UI.ViewModels;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrdersGetterService _ordersGetterService;
        private readonly IOrdersAdderService _ordersAdderService;
        private readonly IOrdersUpdaterService _ordersUpdaterService;
        private readonly IOrdersDeleterService _ordersDeleterService;

        private readonly IOrderItemsGetterService _orderItemsGetterService;
        private readonly IOrderItemsAdderService _orderItemsAdderService;
        private readonly IOrderItemsUpdaterService _orderItemsUpdaterService;

        private readonly IProductsGetterService _productsGetterService;

        private readonly ICustomersGetterService _customersGetterService;


        public OrdersController(IOrdersGetterService ordersGetterService, 
            IOrdersAdderService ordersAdderService, 
            IOrdersUpdaterService ordersUpdaterService, 
            IOrdersDeleterService ordersDeleterService, 
            IOrderItemsGetterService orderItemsGetterService, 
            IOrderItemsAdderService orderItemsAdderService, 
            IOrderItemsUpdaterService orderItemsUpdaterService, 
            IProductsGetterService productsGetterService, 
            ICustomersGetterService customersGetterService)
        {
            _ordersGetterService = ordersGetterService;
            _ordersAdderService = ordersAdderService;
            _ordersUpdaterService = ordersUpdaterService;
            _ordersDeleterService = ordersDeleterService;

            _orderItemsGetterService = orderItemsGetterService;
            _orderItemsAdderService = orderItemsAdderService;
            _orderItemsUpdaterService = orderItemsUpdaterService;

            _productsGetterService = productsGetterService;

            _customersGetterService = customersGetterService;
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _ordersGetterService.GetAllOrders();
            var orderItems = await _orderItemsGetterService.GetAllOrderItems();

            List<OrderWithProductsViewModel> model = new List<OrderWithProductsViewModel>();
            foreach (var order in orders)
            {
                OrderWithProductsViewModel c = new OrderWithProductsViewModel()
                {
                    Order = order,
                    Items = orderItems.Where(i => i.OrderID == order.OrderID).ToList()
                };
                model.Add(c);
            }

            return View(model);
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var customers = await _customersGetterService.GetAllCustomers();
            ViewBag.Customers = customers.Select(s => new SelectListItem()
            {
                Value = s.CustomerID.ToString(),
                Text = s.CustomerName
            });
            ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
            return View();
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(RegisterOrderViewModel registerOrderViewModel)
        {
            if (!ModelState.IsValid)
            {
                var customers = await _customersGetterService.GetAllCustomers();
                ViewBag.Customers = customers.Select(s => new SelectListItem()
                {
                    Value = s.CustomerID.ToString(),
                    Text = s.CustomerName
                });
                ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");

                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(registerOrderViewModel);
            }

            if (registerOrderViewModel is null || registerOrderViewModel.Items is null)
            {
                TempData["ErrorMessage"] = $"Order could not be registered.";
                return RedirectToAction("Index", "Orders");
            }
            var order = await _ordersAdderService.AddOrder(registerOrderViewModel.OrderAddRequest);

            registerOrderViewModel.Items.ForEach(i => i.OrderID = order.OrderID);
            await _orderItemsAdderService.AddMultipleOrderItems(registerOrderViewModel.Items);

            TempData["InfoMessage"] = $"Order has been registered successfully.";
            return RedirectToAction("Index", "Orders");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var foundOrder = await _ordersGetterService.GetOrderByID(id);
            var orderUpdateRequest = foundOrder?.ToOrderUpdateRequest();
            var orderItems = await _orderItemsGetterService.GetAllOrderItemsFromOrder(id);

            var customers = await _customersGetterService.GetAllCustomers();
            ViewBag.Customers = customers.Select(s => new SelectListItem()
            {
                Value = s.CustomerID.ToString(),
                Text = s.CustomerName
            });

            var updateOrderWithProductsModel = new UpdateOrderWithProductsViewModel()
            {
                Order = orderUpdateRequest,
                Items = orderItems?.Select(i => i?.ToOrderItemUpdateRequest()).ToList()
            };

            ViewBag.Products = await _productsGetterService.GetAllProducts();

            return View(updateOrderWithProductsModel);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateOrderWithProductsViewModel updateOrderWithProductsModel)
        {
            if (!ModelState.IsValid)
            {
                var customers = await _customersGetterService.GetAllCustomers();
                ViewBag.Customers = customers.Select(s => new SelectListItem()
                {
                    Value = s.CustomerID.ToString(),
                    Text = s.CustomerName
                });

                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(updateOrderWithProductsModel);
            }

            await _ordersUpdaterService.UpdateOrder(updateOrderWithProductsModel.Order);
            await _orderItemsUpdaterService.UpdateMultipleOrderItems(updateOrderWithProductsModel.Items);
            TempData["InfoMessage"] = "Order data have been updated successfully.";

            return RedirectToAction("Index", "Orders");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("[action]/{orderID}")]
        [HttpDelete]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(Guid orderID)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(orderID);
            }

            if (orderID == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Order data could not be deleted.";
                return PartialView("_errorToastPartialView");
            }

            await _ordersDeleterService.DeleteOrderByID(orderID);

            ViewData["InfoMessage"] = "Order data have been deleted successfully.";
            return PartialView("_infoToastPartialView");
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [Route("get-product/{index}")]
        [HttpGet]
        public IActionResult GetProduct(int index)
        {
            return ViewComponent("AddOrderItem", new { index = index });
        }

        [Authorize(Roles = "Administrator,Manager")]
        [Route("Update/get-update-product/{index}/{orderID}")]
        [HttpGet]
        public IActionResult GetUpdateProduct(int index, Guid orderID)
        {
            return ViewComponent("UpdateOrderItem", new { index = index, orderID = orderID });
        }

        [Authorize(Roles = "Administrator,Manager,Sales")]
        [Route("[action]/{id}")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            await _ordersUpdaterService.CancelOrder(id);
            TempData["InfoMessage"] = $"Order has been cancelled successfully.";
            return RedirectToAction("Index", "Orders");
        }
    }
}
