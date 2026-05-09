using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [Route("/")]
        [Route("[action]")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
