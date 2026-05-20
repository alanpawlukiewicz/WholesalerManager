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

namespace WholesalerManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
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
