using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administrator")]
    [Route("[area]/[controller]/[action]")]
    public class HomeController : Controller
    {
        [HttpGet("/Administrator")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
