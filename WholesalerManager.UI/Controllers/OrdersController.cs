using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.UI.Models;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrdersGetterService _ordersGetterService;
        private readonly IOrderItemsGetterService _orderItemsGetterService;
        private readonly IOrdersDeleterService _ordersDeleterService;

        public OrdersController(IOrdersGetterService ordersGetterService, IOrderItemsGetterService orderItemsGetterService, IOrdersDeleterService ordersDeleterService)
        {
            _ordersGetterService = ordersGetterService;
            _orderItemsGetterService = orderItemsGetterService;
            _ordersDeleterService = ordersDeleterService;
        }

        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var orders = await _ordersGetterService.GetAllOrders();
            var orderItems = await _orderItemsGetterService.GetAllOrderItems();

            List<OrderWithProductsModel> model = new List<OrderWithProductsModel>();
            foreach (var order in orders)
            {
                OrderWithProductsModel c = new OrderWithProductsModel()
                {
                    Order = order,
                    Items = orderItems.Where(i => i.OrderID == order.OrderID).ToList()
                };
                model.Add(c);
            }

            return View(model);
        }

        [Route("[action]/{orderID}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Order data could not be deleted.";
                return PartialView("_errorToastPartialView");
            }

            await _ordersDeleterService.DeleteOrderByID(orderID);

            ViewData["InfoMessage"] = "Order data have been deleted successfully.";
            return PartialView("_infoToastPartialView");
        }
    }
}
