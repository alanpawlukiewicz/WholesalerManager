using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;
using WholesalerManager.UI.Helpers;

namespace WholesalerManager.UI.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administrator")]
    [Route("[area]/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly IUsersGetterService _usersGetterService;
        private readonly IUsersUpdaterService _usersUpdaterService;
        private readonly IUsersRegistrationService _usersRegistrationService;
        private readonly IUsersDeleterService _usersDeleterService;

        private readonly IEmailService _emailService;

        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IUsersGetterService usersGetterService, IUsersUpdaterService usersUpdaterService, IUsersRegistrationService usersRegistrationService, IUsersDeleterService usersDeleterService, IEmailService emailService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _usersGetterService = usersGetterService;
            _usersUpdaterService = usersUpdaterService;
            _usersRegistrationService = usersRegistrationService;
            _usersDeleterService = usersDeleterService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<UserResponse> users;
            if (filter is not null && propertyName is not null)
            {
                users = await _usersGetterService.GetFilteredUsers(propertyName, filter, ignoreCase ?? false);
            }

            else if (sortOrder is not null && propertyName is not null)
            {
                users = await _usersGetterService.GetSortedUsers(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                users = await _usersGetterService.GetAllUsersAsync();
            }

            ViewBag.FieldNames = new List<string>{ nameof(UserResponse.FirstName),
                nameof(UserResponse.LastName), nameof(UserResponse.UserName), nameof(UserResponse.Email), nameof(UserResponse.PhoneNumber) };

            return View(users);
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
            bool passwordNotSet = string.IsNullOrWhiteSpace(registerData.Password);
            var userResponse = await _usersRegistrationService.RegisterUserAsync(registerData);

            if (userResponse is null)
            {
                _logger.LogWarning("User registration failed for {Email}", registerData.Email);
                ModelState.AddModelError("Register", "An error occurred while creating new user.");
                ViewBag.Errors = ModelState.GetErrorMessages();

                return View(registerData);
            }

            if (passwordNotSet)
            {
                var token = await _usersGetterService.GeneratePasswordResetTokenAsync(userResponse.Id);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                string? resetLink = Url.Action(
                                    action: "SetPassword",
                                    controller: "Account",
                                    values: new { email = registerData.Email, token = encodedToken },
                                    protocol: Request.Scheme);

                if (resetLink == null)
                {
                    _logger.LogError("Failed to generate password reset link for user {Email}", registerData.Email);
                    ModelState.AddModelError("Register", "An error occurred while generating the password reset link. Please try again.");
                    ViewBag.Errors = ModelState.GetErrorMessages();
                    return View(registerData);
                }

                await _emailService.SendEmailAsync(registerData.Email, "Your wholesaler employee account has been created", $"Your account has been created with username {userResponse.UserName}.\nPlease set your password by clicking <a href='{resetLink}'>here</a>.");
            }

            _logger.LogInformation("User {UserName} registered successfully with role {UserType}", userResponse.UserName, registerData.UserType);
            TempData["InfoMessage"] = "User successfully registered.";
            return RedirectToAction("Index", "Home");


        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            UserResponse? userResponse = await _usersGetterService.GetUserByIdAsync(id);
            if (userResponse == null)
            {
                _logger.LogWarning("Attempt to edit non-existent user with ID {UserId}", id);
                return NotFound();
            }

            return View(userResponse.ToUserEditRequest());
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Edit(Guid id, UserEditRequest editedUser)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid edit attempt for user with ID {UserId}: {Errors}", id, ModelState.GetErrorMessages());
                ViewBag.Errors = ModelState.GetErrorMessages();
                return View(editedUser);
            }

            var result = await _usersUpdaterService.UpdateUserAsync(editedUser);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} updated successfully", editedUser.UserName);
                TempData["InfoMessage"] = "User successfully updated.";
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("Failed to update user {UserName}: {Errors}", editedUser.UserName, result.Errors.Select(e => e.Description));
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("Edit", error.Description);
            }

            ViewBag.Errors = ModelState.GetErrorMessages();
            return View(editedUser);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> ChangeEnabledStatus(Guid id)
        {
            var result = await _usersUpdaterService.ChangeEnabledStatus(id);
            if (result)
            {
                _logger.LogInformation("User with ID {UserId} enabled status updated successfully", id);
                TempData["InfoMessage"] = "User enabled status updated successfully.";
            }
            else
            {
                _logger.LogWarning("Failed to update enabled status for user with ID {UserId}", id);
                TempData["ErrorMessage"] = "Failed to update user enabled status.";
            }

            return RedirectToAction("Index", "Account");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> RemoveAccount(Guid id)
        {
            var result = await _usersDeleterService.DeleteUser(id);
            if (result)
            {
                _logger.LogInformation("User with ID {UserId} deleted successfully", id);
                TempData["InfoMessage"] = "User deleted successfully.";
            }
            else
            {
                _logger.LogWarning("Failed to delete user with ID {UserId}", id);
                TempData["ErrorMessage"] = "Failed to delete user.";
            }
            return RedirectToAction("Index", "Account");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> ResetAccountPassword(Guid id)
        {
            var matchingUser = await _usersGetterService.GetUserByIdAsync(id);
            if (matchingUser is null)
            {
                _logger.LogInformation("Failed to find user with id:{userID}", id);
                TempData["ErrorMessage"] = "Failed to find user.";
                return RedirectToAction("Index", "Account");
            }

            var token = await _usersGetterService.GeneratePasswordResetTokenAsync(id);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            string? resetLink = Url.Action(
                                action: "SetPassword",
                                controller: "Account",
                                values: new { email = matchingUser.Email, token = encodedToken },
                                protocol: Request.Scheme);

            if (resetLink == null)
            {
                _logger.LogError("Failed to generate password reset link for user {Email}", matchingUser.Email);
                ModelState.AddModelError("Register", "An error occurred while generating the password reset link. Please try again.");
                ViewBag.Errors = ModelState.GetErrorMessages();

                return RedirectToAction("Edit", "Account", new { id });
            }

            var result = await _usersUpdaterService.MakeUserChangePassword(id);

            if (!result.Succeeded)
            {
                _logger.LogInformation("Failed to update user with id:{userID}.", id);
                TempData["ErrorMessage"] = "Failed to update user.";
                return RedirectToAction("Index", "Account");
            }

            await _emailService.SendEmailAsync(matchingUser.Email, "Your wholesaler employee account password has been reseted.", $"Please reset your password by clicking <a href='{resetLink}'>here</a>.");

            _logger.LogInformation("User's password with id {id} has benn successfully reseted.", id);
            TempData["InfoMessage"] = "Password reset request has been successfully sent.";
            return RedirectToAction("Index", "Account");
        }
    }
}
