using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Route("/")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View();
            }

            if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Administrator.ToString()))
            {
                return RedirectToAction("Index", "Home", new { area = "Administrator" });
            }

            return View();
        }
    }
}
