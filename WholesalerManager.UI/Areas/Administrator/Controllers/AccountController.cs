using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts;
using WholesalerManager.Core.Helpers;

namespace WholesalerManager.UI.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administrator")]
    [Route("[area]/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IUserNameGeneratorService _userNameGeneratorService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IEmailService _emailService;

        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager, IUserNameGeneratorService userNameGeneratorService, IPasswordGeneratorService passwordGeneratorService, IEmailService emailService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userNameGeneratorService = userNameGeneratorService;
            _passwordGeneratorService = passwordGeneratorService;
            _emailService = emailService;
            _logger = logger;
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
                _logger.LogWarning("Invalid registration attempt: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(registerData);
            }

            if (registerData.UserType == UserTypeOptions.Administrator && !User.IsInRole(UserTypeOptions.Administrator.ToString()))
            {
                _logger.LogWarning("Unauthorized attempt to create an administrator account by user {UserName}", User.Identity?.Name);
                ModelState.AddModelError("Register", "Only administrators can create new administrator accounts.");
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(registerData);
            }

            bool passwordNotSet = string.IsNullOrWhiteSpace(registerData.Password);

            ApplicationUser user = new ApplicationUser()
            {
                FirstName = registerData.FirstName,
                LastName = registerData.LastName,
                MustChangePassword = passwordNotSet,
                Email = registerData.Email,
                UserName = _userNameGeneratorService.Generate(registerData.FirstName, registerData.LastName),
                PhoneNumber = registerData.Phone
            };

            // Generate a random password if not provided
            string password = passwordNotSet ? _passwordGeneratorService.Generate() : registerData.Password!;

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Add role if it doesn't exist
                if (_roleManager.FindByNameAsync(registerData.UserType.ToString()).Result == null)
                {
                    await _roleManager.CreateAsync(new ApplicationRole() { Name = registerData.UserType.ToString() });
                }

                // Assign the user to the role
                await _userManager.AddToRoleAsync(user, registerData.UserType.ToString());

                // If the password was not set by the admin, send an email with a link to set the password
                if (passwordNotSet)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    string? resetLink = Url.Action(
                                        action: "SetPassword",
                                        controller: "Account",
                                        values: new { email = user.Email, token = encodedToken },
                                        protocol: Request.Scheme);

                    if (resetLink == null)
                    {
                        _logger.LogError("Failed to generate password reset link for user {UserName}", user.UserName);
                        ModelState.AddModelError("Register", "An error occurred while generating the password reset link. Please try again.");
                        ViewBag.Errors = ModelState.GetErrorMessages();
                        return View(registerData);
                    }

                    await _emailService.SendEmailAsync(user.Email, "Your wholesaler employee account has been created", $"Your account has been created with username {user.UserName}.\nPlease set your password by clicking <a href='{resetLink}'>here</a>.");

                }

                _logger.LogInformation("User {UserName} registered successfully with role {UserType}", user.UserName, registerData.UserType);
                TempData["InfoMessage"] = "User successfully registered.";
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("User registration failed for {UserName}: {Errors}", user.UserName, result.Errors.Select(e => e.Description));
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description);
            }
            ViewBag.Errors = ModelState.GetErrorMessages();

            return View(registerData);
        }

    }
}
