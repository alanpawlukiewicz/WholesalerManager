using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.UI.Helpers;
using WholesalerManager.UI.ViewModels;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrdersGetterService _ordersGetterService;
        private readonly IOrdersDeleterService _ordersDeleterService;
        private readonly IOrdersUpdaterService _ordersUpdaterService;
        private readonly IOrderRegistrationService _orderRegistrationService;
        private readonly IOrderUpdateCoordinatorService _orderUpdateCoordinatorService;

        private readonly IOrderItemsGetterService _orderItemsGetterService;

        private readonly IProductsGetterService _productsGetterService;

        private readonly ICustomersGetterService _customersGetterService;

        private readonly ILogger<OrdersController> _logger;


        public OrdersController(IOrdersGetterService ordersGetterService,
            IOrdersAdderService ordersAdderService,
            IOrdersDeleterService ordersDeleterService,
            IOrdersUpdaterService ordersUpdaterService,
            IOrderRegistrationService orderRegistrationService,
            IOrderUpdateCoordinatorService orderUpdateCoordinatorService,
            IOrderItemsGetterService orderItemsGetterService,
            IProductsGetterService productsGetterService,
            ICustomersGetterService customersGetterService,
            ILogger<OrdersController> logger)
        {
            _ordersGetterService = ordersGetterService;
            _ordersDeleterService = ordersDeleterService;
            _ordersUpdaterService = ordersUpdaterService;
            _orderRegistrationService = orderRegistrationService;
            _orderUpdateCoordinatorService = orderUpdateCoordinatorService;

            _orderItemsGetterService = orderItemsGetterService;

            _productsGetterService = productsGetterService;

            _customersGetterService = customersGetterService;

            _logger = logger;
        }

        [Authorize(Roles = "Administrator,Manager,Sales,Operator")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<OrderResponse> orders;

            if (filter is not null && propertyName is not null)
            {
                orders = await _ordersGetterService.GetFilteredOrders(propertyName, filter, ignoreCase ?? false);
            }

            else if (sortOrder is not null && propertyName is not null)
            {
                orders = await _ordersGetterService.GetSortedOrders(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                orders = await _ordersGetterService.GetAllOrders();
            }

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

            ViewBag.FieldNames = new List<string>() { nameof(OrderResponse.CustomerName), nameof(OrderResponse.OrderDate), nameof(OrderResponse.Status), nameof(OrderResponse.TIN) };

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
                _logger.LogWarning("Invalid model state: {Errors}", ModelState.GetErrorMessages());
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
                _logger.LogError("RegisterOrderViewModel or its Items property is null.");
                TempData["ErrorMessage"] = $"Order could not be registered.";
                return RedirectToAction("Index", "Orders");
            }

            await _orderRegistrationService.RegisterFullOrder(registerOrderViewModel.OrderAddRequest, registerOrderViewModel.Items);

            _logger.LogInformation("Order registered successfully with CustomerID: {CustomerID}", registerOrderViewModel.OrderAddRequest?.CustomerID);
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
                Items = orderItems?.Select(i => i.ToOrderItemUpdateRequest()).ToList()
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
                _logger.LogWarning("Invalid model state: {Errors}", ModelState.GetErrorMessages());
                var customers = await _customersGetterService.GetAllCustomers();
                ViewBag.Customers = customers.Select(s => new SelectListItem()
                {
                    Value = s.CustomerID.ToString(),
                    Text = s.CustomerName
                });

                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(updateOrderWithProductsModel);
            }

            await _orderUpdateCoordinatorService.UpdateFullOrder(updateOrderWithProductsModel.Order, updateOrderWithProductsModel.Items);

            _logger.LogInformation("Order updated successfully with OrderID: {OrderID}", updateOrderWithProductsModel.Order?.OrderID);
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
                _logger.LogWarning("Invalid model state: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return RedirectToAction("Index", "Orders");
            }

            if (orderID == Guid.Empty)
            {
                _logger.LogWarning("Invalid order ID: {OrderID}", orderID);
                ViewData["ErrorMessage"] = "Order data could not be deleted.";
                return PartialView("_errorToastPartialView");
            }

            await _ordersDeleterService.DeleteOrderByID(orderID);

            _logger.LogInformation("Order deleted successfully with OrderID: {OrderID}", orderID);
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
        public IActionResult GetUpdateProduct([FromRoute] int index, [FromRoute] Guid orderID)
        {
            return ViewComponent("UpdateOrderItem", new { index = index, orderID = orderID });
        }

        [Authorize(Roles = "Administrator,Sales")]
        [Route("[action]/{id}")]
        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            bool result = await _ordersUpdaterService.CancelOrder(id);
            if (!result)
            {
                _logger.LogWarning("Failed to cancel order with ID: {OrderID}", id);
                TempData["ErrorMessage"] = $"Order could not be cancelled.";
                return RedirectToAction("Index", "Orders");
            }

            _logger.LogInformation("Order cancelled successfully with OrderID: {OrderID}", id);
            TempData["InfoMessage"] = $"Order has been cancelled successfully.";
            return RedirectToAction("Index", "Orders");
        }

        [Authorize(Roles = "Administrator,Manager,Operator")]
        [Route("[action]/{orderID}/{status}")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid orderID, [FromRoute] OrderStatus status)
        {
            bool result = await _ordersUpdaterService.UpdateOrderStatus(orderID, status);
            if (!result)
            {
                _logger.LogWarning("Failed to update order status for order with ID: {OrderID}", orderID);
                TempData["ErrorMessage"] = $"Order status could not be updated.";
                return PartialView("_errorToastPartialView");
            }

            _logger.LogInformation("Order status updated successfully for order with ID: {OrderID}", orderID);
            TempData["InfoMessage"] = $"Order status has been updated successfully.";
            return PartialView("_infoToastPartialView");
        }
    }
}
