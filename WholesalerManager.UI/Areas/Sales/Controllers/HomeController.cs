using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.Areas.Sales.Controllers
{
    [Authorize(Roles = "Sales")]
    [Area("Sales")]
    [Route("[area]/[controller]/[action]")]
    public class HomeController : Controller
    {
        [HttpGet("/Sales")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
