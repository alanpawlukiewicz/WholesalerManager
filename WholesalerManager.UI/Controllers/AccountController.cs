using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.ServiceContracts;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserNameGeneratorService _userNameGeneratorService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, IUserNameGeneratorService userNameGeneratorService, IPasswordGeneratorService passwordGeneratorService, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _userNameGeneratorService = userNameGeneratorService;
            _passwordGeneratorService = passwordGeneratorService;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
                TempData["InfoMessage"] = "User successfully registered.";
                return RedirectToAction("Index", "Home");
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description);
            }

            return View(registerData);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginData, string? ReturnURL)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
                return View(loginData);
            }

            if (loginData.UserName == null || loginData.Password == null)
            {
                ModelState.AddModelError("Login", "Username and password are required.");
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
                return View(loginData);
            }

            var result = await _signInManager.PasswordSignInAsync(loginData.UserName, loginData.Password, isPersistent: loginData.KeepSignedIn, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                TempData["InfoMessage"] = "Successfully logged in.";
                if (!string.IsNullOrWhiteSpace(ReturnURL) && Url.IsLocalUrl(ReturnURL))
                {
                    return LocalRedirect(ReturnURL);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Login", "Invalid username or password.");

            return View(loginData);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["InfoMessage"] = "Successfully logged out.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return Content("Access denied.");
        }
    }
}
