using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.Areas.Operator.Controllers
{
    [Authorize(Roles = "Operator")]
    [Area("Operator")]
    [Route("[area]/[controller]/[action]")]
    public class HomeController : Controller
    {
        [HttpGet("/Operator")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
