using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts;
using WholesalerManager.UI.Filters.ActionFilters;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IUserNameGeneratorService _userNameGeneratorService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IEmailService _emailService;

        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IUserNameGeneratorService userNameGeneratorService, IPasswordGeneratorService passwordGeneratorService, SignInManager<ApplicationUser> signInManager, IEmailService emailService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userNameGeneratorService = userNameGeneratorService;
            _passwordGeneratorService = passwordGeneratorService;
            _emailService = emailService;
            _signInManager = signInManager;
            _logger = logger;
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Authorize(Roles = "Administrator,Manager")]
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
                                        action: nameof(SetPassword),
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

        [Authorize("RequireNotAuthenticated")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Authorize("RequireNotAuthenticated")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginData, string? ReturnURL)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(loginData);
            }

            if (loginData.UserName == null || loginData.Password == null)
            {
                _logger.LogWarning("Login attempt with missing username or password.");
                ModelState.AddModelError("Login", "Username and password are required.");
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(loginData);
            }

            var result = await _signInManager.PasswordSignInAsync(loginData.UserName, loginData.Password, isPersistent: loginData.KeepSignedIn, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid login attempt for username {UserName}.", loginData.UserName);
                ModelState.AddModelError("Login", "Invalid username or password.");
                ViewBag.Errors = ModelState.GetErrorMessages();

                return View(loginData);
            }

            _logger.LogInformation("User {UserName} logged in successfully.", loginData.UserName);
            TempData["InfoMessage"] = "Successfully logged in.";
            if (!string.IsNullOrWhiteSpace(ReturnURL) && Url.IsLocalUrl(ReturnURL))
            {
                return LocalRedirect(ReturnURL);
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserName} logged out successfully.", User.Identity?.Name);
            TempData["InfoMessage"] = "Successfully logged out.";
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "You are not authorized to access this resource.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize("RequireNotAuthenticated")]
        [HttpGet]
        public IActionResult SetPassword(string email, string token)
        {
            var model = new ResetPasswordDTO
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        [Authorize("RequireNotAuthenticated")]
        [HttpPost]
        public async Task<IActionResult> SetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid password reset attempt: {Errors}", ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Password reset attempt for non-existent email: {Email}", model.Email);
                ModelState.AddModelError("SetPassword", "Invalid email address.");
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(model);
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password!);
            if (result.Succeeded)
            {
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {UserName} successfully set a new password.", user.UserName);
                TempData["InfoMessage"] = "Your password has been set successfully.";
                return RedirectToAction("Login");
            }

            _logger.LogWarning("Failed to set new password for user {UserName}: {Errors}", user.UserName, result.Errors.Select(e => e.Description));
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("SetPassword", error.Description);
            }
            ViewBag.Errors = ModelState.GetErrorMessages();
            return View(model);
        }
    }
}
