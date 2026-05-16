using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [HttpGet]
        [Route("/Error")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
