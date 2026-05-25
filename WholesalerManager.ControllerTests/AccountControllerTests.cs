using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.ServiceContracts;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;
using WholesalerManager.UI.Controllers;

namespace WholesalerManager.ControllerTests
{


    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IUsersGetterService> _usersGetterServiceMock;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
        private readonly Mock<IAuditLoggerService> _auditLoggerMock;
        private readonly IFixture _fixture;
        private readonly AccountController _sut;

        public AccountControllerTests()
        {
            // UserManager requires several dependencies – use Mock.Of for unused ones
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            _usersGetterServiceMock = new Mock<IUsersGetterService>();
            _loggerMock = new Mock<ILogger<AccountController>>();
            _auditLoggerMock = new Mock<IAuditLoggerService>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _usersGetterServiceMock.Object,
                _loggerMock.Object,
                _auditLoggerMock.Object
            );
        }

        #region Helpers

        private void SetupTempData()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);
        }

        // Creates a valid LoginDTO
        private LoginDTO CreateValidLoginDTO(Action<LoginDTO>? configure = null)
        {
            var dto = _fixture.Build<LoginDTO>()
                .With(d => d.UserName, "testuser")
                .With(d => d.Password, "Password123!")
                .With(d => d.KeepSignedIn, false)
                .Create();

            configure?.Invoke(dto);
            return dto;
        }

        // Creates an enabled ApplicationUser with MustChangePassword = false
        private ApplicationUser CreateEnabledUser(Action<ApplicationUser>? configure = null)
        {
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.IsEnabled, true)
                .With(u => u.MustChangePassword, false)
                .With(u => u.UserName, "testuser")
                .With(u => u.Email, "test@test.com")
                .Create();

            configure?.Invoke(user);
            return user;
        }

        #endregion

        #region Login GET

        [Fact]
        public void Login_Get_ReturnsView()
        {
            // Act
            var result = _sut.Login();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Login POST

        [Fact]
        public async Task Login_Post_InvalidModelState_ReturnsViewWithLoginData()
        {
            // Arrange – simulate invalid model state
            var loginData = CreateValidLoginDTO();
            _sut.ModelState.AddModelError("UserName", "Username is required.");

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – view returned with original DTO, no user lookup performed
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginData);
            _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_Post_NullUsernameOrPassword_ReturnsViewWithError()
        {
            // Arrange – username is null
            var loginData = CreateValidLoginDTO(d => d.UserName = null);

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – view returned with model error added
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginData);
            _sut.ModelState.ContainsKey("Login").Should().BeTrue();
        }

        [Fact]
        public async Task Login_Post_UserNotFound_ReturnsViewWithError()
        {
            // Arrange – no user with given username exists
            var loginData = CreateValidLoginDTO();

            _userManagerMock
                .Setup(m => m.FindByNameAsync(loginData.UserName!))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – view returned with invalid credentials error
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginData);
            _sut.ModelState.ContainsKey("Login").Should().BeTrue();
        }

        [Fact]
        public async Task Login_Post_DisabledUser_ReturnsViewWithError()
        {
            // Arrange – user exists but is disabled
            var loginData = CreateValidLoginDTO();
            var disabledUser = CreateEnabledUser(u => u.IsEnabled = false);

            _userManagerMock
                .Setup(m => m.FindByNameAsync(loginData.UserName!))
                .ReturnsAsync(disabledUser);

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – view returned with disabled account error
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginData);
            _sut.ModelState.ContainsKey("Login").Should().BeTrue();
        }

        [Fact]
        public async Task Login_Post_WrongPassword_ReturnsViewWithError()
        {
            // Arrange – user found but password check fails
            var loginData = CreateValidLoginDTO();
            var user = CreateEnabledUser();

            _userManagerMock
                .Setup(m => m.FindByNameAsync(loginData.UserName!))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(m => m.CheckPasswordSignInAsync(user, loginData.Password!, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            _auditLoggerMock
                .Setup(a => a.LogLoginAttempt(user.Id, user.UserName!, false))
                .ReturnsAsync(It.IsAny<bool>());

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – view returned with invalid credentials error
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginData);
            _sut.ModelState.ContainsKey("Login").Should().BeTrue();
        }

        [Fact]
        public async Task Login_Post_MustChangePassword_RedirectsToSetPassword()
        {
            // Arrange – user must change password on first login
            SetupTempData();

            var loginData = CreateValidLoginDTO();
            var user = CreateEnabledUser(u => u.MustChangePassword = true);
            var resetToken = "reset-token";

            _userManagerMock
                .Setup(m => m.FindByNameAsync(loginData.UserName!))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(m => m.CheckPasswordSignInAsync(user, loginData.Password!, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _auditLoggerMock
                .Setup(a => a.LogLoginAttempt(user.Id, user.UserName!, true))
                .ReturnsAsync(It.IsAny<bool>());

            _usersGetterServiceMock
                .Setup(s => s.GeneratePasswordResetTokenAsync(user.Id))
                .ReturnsAsync(resetToken);

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – redirected to SetPassword action
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("SetPassword");
            redirectResult.ControllerName.Should().Be("Account");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_RedirectsToHome()
        {
            // Arrange – successful login, no password change required
            SetupTempData();

            var loginData = CreateValidLoginDTO();
            var user = CreateEnabledUser();

            _userManagerMock
                .Setup(m => m.FindByNameAsync(loginData.UserName!))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(m => m.CheckPasswordSignInAsync(user, loginData.Password!, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _signInManagerMock
                .Setup(m => m.PasswordSignInAsync(user.UserName!, loginData.Password!, loginData.KeepSignedIn, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _auditLoggerMock
                .Setup(a => a.LogLoginAttempt(user.Id, user.UserName!, true))
                .ReturnsAsync(It.IsAny<bool>());

            // Act
            var result = await _sut.Login(loginData, null);

            // Assert – redirected to Home/Index after successful login
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Login_Post_ValidCredentialsWithReturnURL_RedirectsToReturnURL()
        {
            // Arrange – successful login with a valid local return URL
            SetupTempData();

            var loginData = CreateValidLoginDTO();
            var user = CreateEnabledUser();
            var returnUrl = "/Products/Index";

            _userManagerMock
                .Setup(m => m.FindByNameAsync(loginData.UserName!))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(m => m.CheckPasswordSignInAsync(user, loginData.Password!, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _signInManagerMock
                .Setup(m => m.PasswordSignInAsync(user.UserName!, loginData.Password!, loginData.KeepSignedIn, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _auditLoggerMock
                .Setup(a => a.LogLoginAttempt(user.Id, user.UserName!, true))
                .ReturnsAsync(It.IsAny<bool>());

            // Set up URL helper to treat the return URL as local
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);
            _sut.Url = urlHelperMock.Object;

            // Act
            var result = await _sut.Login(loginData, returnUrl);

            // Assert – redirected to the local return URL
            var redirectResult = result.Should().BeOfType<LocalRedirectResult>().Subject;
            redirectResult.Url.Should().Be(returnUrl);
        }

        [Fact]
        public async Task Login_Post_LoginWithEmail_LooksUpUserByEmail()
        {
            // Arrange – username is an email address
            SetupTempData();

            var loginData = CreateValidLoginDTO(d => d.UserName = "test@test.com");
            var user = CreateEnabledUser(u => u.Email = "test@test.com");

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(loginData.UserName!))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(m => m.CheckPasswordSignInAsync(user, loginData.Password!, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _signInManagerMock
                .Setup(m => m.PasswordSignInAsync(user.UserName!, loginData.Password!, loginData.KeepSignedIn, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _auditLoggerMock
                .Setup(a => a.LogLoginAttempt(user.Id, user.UserName!, true))
                .ReturnsAsync(It.IsAny<bool>());

            // Act
            await _sut.Login(loginData, null);

            // Assert – FindByEmailAsync called instead of FindByNameAsync
            _userManagerMock.Verify(m => m.FindByEmailAsync(loginData.UserName!), Times.Once);
            _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Logout

        [Fact]
        public async Task Logout_SignsOutAndRedirectsToHome()
        {
            // Arrange
            SetupTempData();

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var tempDataProvider = Mock.Of<ITempDataProvider>();
            _sut.TempData = new TempDataDictionary(httpContext, tempDataProvider);

            _signInManagerMock
                .Setup(m => m.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Logout();

            // Assert – signed out and redirected to Home
            _signInManagerMock.Verify(m => m.SignOutAsync(), Times.Once);
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        #endregion

        #region AccessDenied

        [Fact]
        public void AccessDenied_RedirectsToHomeWithErrorMessage()
        {
            // Arrange
            SetupTempData();

            // Act
            var result = _sut.AccessDenied();

            // Assert – redirected to Home with error message in TempData
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        #endregion

        #region SetPassword GET

        [Fact]
        public void SetPassword_Get_ReturnsViewWithModel()
        {
            // Arrange
            var email = "test@test.com";
            var token = "some-token";

            // Act
            var result = _sut.SetPassword(email, token);

            // Assert – view returned with ResetPasswordDTO pre-filled
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<ResetPasswordDTO>().Subject;
            model.Email.Should().Be(email);
            model.Token.Should().Be(token);
        }

        #endregion

        #region SetPassword POST

        [Fact]
        public async Task SetPassword_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = _fixture.Create<ResetPasswordDTO>();
            _sut.ModelState.AddModelError("Password", "Password is required.");

            // Act
            var result = await _sut.SetPassword(model);

            // Assert – view returned, user manager never called
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _userManagerMock.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SetPassword_Post_UserNotFound_ReturnsViewWithError()
        {
            // Arrange – no user with given email exists
            var model = _fixture.Build<ResetPasswordDTO>()
                .With(m => m.Email, "notfound@test.com")
                .With(m => m.Token, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("token")))
                .With(m => m.Password, "NewPass123!")
                .Create();

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _sut.SetPassword(model);

            // Assert – view returned with error, password reset never attempted
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _sut.ModelState.ContainsKey("SetPassword").Should().BeTrue();
            _userManagerMock.Verify(m => m.ResetPasswordAsync(
                It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SetPassword_Post_ResetFails_ReturnsViewWithErrors()
        {
            // Arrange – password reset fails (e.g. token expired or too weak)
            var rawToken = "valid-token";
            var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
                System.Text.Encoding.UTF8.GetBytes(rawToken));

            var model = _fixture.Build<ResetPasswordDTO>()
                .With(m => m.Email, "test@test.com")
                .With(m => m.Token, encodedToken)
                .With(m => m.Password, "NewPass123!")
                .Create();

            var user = CreateEnabledUser(u => u.Email = model.Email);

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ResetPasswordAsync(user, rawToken, model.Password!))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Code = "WeakPassword", Description = "Password is too weak." }));

            // Act
            var result = await _sut.SetPassword(model);

            // Assert – view returned with identity errors
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _sut.ModelState.ContainsKey("SetPassword").Should().BeTrue();
        }

        [Fact]
        public async Task SetPassword_Post_ResetSucceeds_RedirectsToLogin()
        {
            // Arrange – password reset succeeds
            SetupTempData();

            var rawToken = "valid-token";
            var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
                System.Text.Encoding.UTF8.GetBytes(rawToken));

            var model = _fixture.Build<ResetPasswordDTO>()
                .With(m => m.Email, "test@test.com")
                .With(m => m.Token, encodedToken)
                .With(m => m.Password, "NewPass123!")
                .Create();

            var user = CreateEnabledUser(u =>
            {
                u.Email = model.Email;
                u.MustChangePassword = true;
            });

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ResetPasswordAsync(user, rawToken, model.Password!))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.SetPassword(model);

            // Assert – MustChangePassword cleared, redirected to Login
            user.MustChangePassword.Should().BeFalse();
            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
            _sut.TempData["InfoMessage"].Should().NotBeNull();
        }

        #endregion
    }
}
