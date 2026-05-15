using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserNameGeneratorService _userNameGeneratorService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;

        public AccountController(UserManager<ApplicationUser> userManager, IUserNameGeneratorService userNameGeneratorService, IPasswordGeneratorService passwordGeneratorService)
        {
            _userManager = userManager;
            _userNameGeneratorService = userNameGeneratorService;
            _passwordGeneratorService = passwordGeneratorService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerData)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
                return View(registerData);
            }
            ApplicationUser user = new ApplicationUser()
            {
                FirstName = registerData.FirstName,
                LastName = registerData.LastName,
                Email = registerData.Email,
                UserName = _userNameGeneratorService.Generate(registerData.FirstName, registerData.LastName),
                PhoneNumber = registerData.Phone
            };

            // Generate a random password if not provided
            string password = string.IsNullOrWhiteSpace(registerData.Password) ? _passwordGeneratorService.Generate() : registerData.Password;

            IdentityResult result = await _userManager.CreateAsync(user, password);

            // TODO: Send the generated password to the user's email if it was generated automatically

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach(IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(registerData);
        }
        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult Login()
        //{
        //    return View();
        //}
    }
}
