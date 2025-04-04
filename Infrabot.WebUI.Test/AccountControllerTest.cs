using Infrabot.Common.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Infrabot.WebUI.Controllers;
using Microsoft.Extensions.Logging;
using Infrabot.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Infrabot.WebUI.Test
{
    [TestFixture]
    public class AccountControllerTest
    {
        private DbContextOptions<InfrabotContext> _options;
        private readonly ILogger<AccountController> _logger = Mock.Of<ILogger<AccountController>>();
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {

            // Use SQLite in-memory provider to support relational methods
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open(); // Keep connection open for the in-memory DB to persist

            _options = new DbContextOptionsBuilder<InfrabotContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new InfrabotContext(_options);
            context.Database.EnsureCreated();

            context.TelegramUsers.RemoveRange(context.TelegramUsers);
            context.Users.RemoveRange(context.Users);
            context.AuditLogs.RemoveRange(context.AuditLogs);
            context.EventLogs.RemoveRange(context.EventLogs);
            context.Configurations.RemoveRange(context.Configurations);
            context.Groups.RemoveRange(context.Groups);
            context.Plugins.RemoveRange(context.Plugins);
            context.UserGroups.RemoveRange(context.UserGroups);
            context.GroupPlugins.RemoveRange(context.GroupPlugins);
            context.TelegramMessages.RemoveRange(context.TelegramMessages);
            context.PermissionAssignments.RemoveRange(context.PermissionAssignments);
            context.HealthChecks.RemoveRange(context.HealthChecks);

            // Seed Users
            context.Users.Add(new User
            {
                Id = 1,
                Name = "Super",
                Surname = "Admin",
                Login = "admin",
                Email = "admin@company.com",
                Password = "password",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                Phone = "",
                IsADIntegrated = false,
                Enabled = true
            });

            // Seed Configuration
            context.Configurations.Add(new Configuration
            {
                Id = 1,
                IsADEnabled = false,
                ADLogin = "administrator",
                ADPassword = "example.lan",
                ADServer = "example.lan",
                ADDomainName = "example.lan",
                TelegramBotToken = "1988239795:AAEgiZPohgsv6Tvmi1qhjIKbcKYnehbRjMw",
                TelegramEnableEmergency = true,
                TelegramEnableShowMyId = true,
                TelegramPowerShellPath = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
                TelegramPowerShellArguments = "-ExecutionPolicy Unrestricted -NoProfile",
                TelegramLinuxShellPath = "/bin/bash",
                TelegramLinuxShellArguments = "",
                TelegramPythonPath = "/usr/bin/python",
                TelegramPythonArguments = "",
                TelegramResultMaxLength = 12000,
                PasswordPolicyMinLength = 6,
                PasswordPolicyContainLowerCase = false,
                PasswordPolicyContainUpperCase = false,
                PasswordPolicyContainNumber = false,
                PasswordPolicyContainSpecialCharacter = false,
                UpdatedDate = DateTime.Now
            });

            context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        #region Log In Tests

        [Test]
        public void LogIn_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["LoginOrPasswordIncorrect"] = false;
                controller.TempData["LoginDenied"] = false;
                controller.TempData["LoginDataIsNotValid"] = false;
                controller.TempData["ADAuthFailed"] = false;
                controller.TempData["LoginDeniedForApiUser"] = false;

                // Act
                var result = controller.LogIn();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
            }
        }

        [Test]
        public void LogIn_ReturnsView_LoginOrPasswordIncorrect()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["LoginOrPasswordIncorrect"] = true;
                controller.TempData["LoginDenied"] = false;
                controller.TempData["LoginDataIsNotValid"] = false;
                controller.TempData["ADAuthFailed"] = false;
                controller.TempData["LoginDeniedForApiUser"] = false;

                // Act
                var result = controller.LogIn();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(controller.TempData["LoginOrPasswordIncorrect"], Is.EqualTo(true));
            }
        }

        [Test]
        public void LogIn_ReturnsView_LoginDenied()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["LoginOrPasswordIncorrect"] = false;
                controller.TempData["LoginDenied"] = true;
                controller.TempData["LoginDataIsNotValid"] = false;
                controller.TempData["ADAuthFailed"] = false;
                controller.TempData["LoginDeniedForApiUser"] = false;

                // Act
                var result = controller.LogIn();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(controller.TempData["LoginDenied"], Is.EqualTo(true));
            }
        }


        [Test]
        public void LogIn_ReturnsView_LoginDataIsNotValid()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["LoginOrPasswordIncorrect"] = false;
                controller.TempData["LoginDenied"] = false;
                controller.TempData["LoginDataIsNotValid"] = true;
                controller.TempData["ADAuthFailed"] = false;
                controller.TempData["LoginDeniedForApiUser"] = false;

                // Act
                var result = controller.LogIn();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(controller.TempData["LoginDataIsNotValid"], Is.EqualTo(true));
            }
        }

        [Test]
        public void LogIn_ReturnsView_ADAuthFailed()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["LoginOrPasswordIncorrect"] = false;
                controller.TempData["LoginDenied"] = false;
                controller.TempData["LoginDataIsNotValid"] = false;
                controller.TempData["ADAuthFailed"] = true;
                controller.TempData["LoginDeniedForApiUser"] = false;

                // Act
                var result = controller.LogIn();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(controller.TempData["ADAuthFailed"], Is.EqualTo(true));
            }
        }


        [Test]
        public void LogIn_ReturnsView_LoginDeniedForApiUser()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["LoginOrPasswordIncorrect"] = false;
                controller.TempData["LoginDenied"] = false;
                controller.TempData["LoginDataIsNotValid"] = false;
                controller.TempData["ADAuthFailed"] = false;
                controller.TempData["LoginDeniedForApiUser"] = true;

                // Act
                var result = controller.LogIn();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(controller.TempData["LoginDeniedForApiUser"], Is.EqualTo(true));
            }
        }

        [Test]
        public async Task LogIn_RedirectToActionAndLoginDataIsNotValid()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                var user = new User
                {
                    Login = "non_existent",
                    Password = "password"
                };

                // Act
                var result = await controller.LogIn(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("LogIn"));
                Assert.That(controller.TempData["LoginDataIsNotValid"], Is.EqualTo(true));
            }
        }

        [Test]
        public async Task LogIn_RedirectToActionHomeIndex()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Mock HttpContext with required services
                var context = new DefaultHttpContext();

                // Set up fake HttpContext and ServiceProvider
                var httpContext = new DefaultHttpContext();

                // Mock IAuthenticationService
                var authService = new Mock<IAuthenticationService>();
                authService
                    .Setup(x => x.SignInAsync(
                        It.IsAny<HttpContext>(),
                        It.IsAny<string>(),
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthenticationProperties>()))
                    .Returns(Task.CompletedTask);

                // Mock IUrlHelperFactory and IUrlHelper
                var urlHelperMock = new Mock<IUrlHelper>();
                urlHelperMock
                    .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                    .Returns("fake-url");

                var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
                urlHelperFactoryMock
                    .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
                    .Returns(urlHelperMock.Object);

                // Create service provider
                var services = new ServiceCollection()
                    .AddSingleton(authService.Object)
                    .AddSingleton(urlHelperFactoryMock.Object)
                    .BuildServiceProvider();

                httpContext.RequestServices = services;

                controller.ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                };

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                var user = new User
                {
                    Login = "admin",
                    Password = "password"
                };

                // Act
                var result = await controller.LogIn(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
                Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
            }
        }

        [Test]
        public async Task LogIn_RedirectsToLogIn_WhenConfigurationIsMissing()
        {
            // Arrange: use fresh context without seeding configuration
            using (var context = new InfrabotContext(_options))
            {
                // Delete any seeded configuration from Setup
                context.Configurations.RemoveRange(context.Configurations);
                await context.SaveChangesAsync();

                var controller = new AccountController(_logger, context);

                // Setup HttpContext and TempData
                var httpContext = new DefaultHttpContext();

                // Add required DI services
                var authServiceMock = new Mock<IAuthenticationService>();
                authServiceMock
                    .Setup(x => x.SignInAsync(
                        It.IsAny<HttpContext>(),
                        It.IsAny<string>(),
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthenticationProperties>()))
                    .Returns(Task.CompletedTask);

                var urlHelperMock = new Mock<IUrlHelper>();
                urlHelperMock
                    .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                    .Returns("fake-url");

                var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
                urlHelperFactoryMock
                    .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
                    .Returns(urlHelperMock.Object);

                var services = new ServiceCollection()
                    .AddSingleton(authServiceMock.Object)
                    .AddSingleton(urlHelperFactoryMock.Object)
                    .BuildServiceProvider();

                httpContext.RequestServices = services;

                controller.ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                };

                controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

                var user = new User
                {
                    Login = "admin",
                    Password = "password"
                };

                // Act
                var result = await controller.LogIn(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("LogIn"));
            }
        }

        #endregion

        #region Log Out Tests

        [Test]
        public async Task LogOut_RedirectToLogInAccount()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new AccountController(_logger, _context);

                // Arrange HttpContext and mock services
                var httpContext = new DefaultHttpContext();

                var authServiceMock = new Mock<IAuthenticationService>();
                authServiceMock
                    .Setup(x => x.SignOutAsync(
                        It.IsAny<HttpContext>(),
                        It.IsAny<string>(),
                        It.IsAny<AuthenticationProperties>()))
                    .Returns(Task.CompletedTask); // mock success

                var urlHelperMock = new Mock<IUrlHelper>();
                urlHelperMock
                    .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                    .Returns("fake-url");

                var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
                urlHelperFactoryMock
                    .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
                    .Returns(urlHelperMock.Object);

                var services = new ServiceCollection()
                    .AddSingleton(authServiceMock.Object)
                    .AddSingleton(urlHelperFactoryMock.Object)
                    .BuildServiceProvider();

                httpContext.RequestServices = services;

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };

                // Act
                var result = await controller.LogOut();

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("LogIn"));
                Assert.That(redirectResult.ControllerName, Is.EqualTo("Account"));
            }
        }

        #endregion

        #region Change Password Tests

        [Test]
        public async Task ChangePassword_ReturnsView_WhenUserIsValid()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new AccountController(_logger, context);

                // Setup HttpContext with authenticated user and claims
                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", "1"),
                        new Claim(ClaimTypes.Name, "admin")
                    }, "TestAuth"))
                };

                // Setup TempData with optional flags
                var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
                {
                    ["OldPasswordIsIncorrect"] = true,
                    ["NewPasswordNotEqualToRepeat"] = true,
                    ["DoesNotMeetComplexityRequirements"] = true,
                    ["SucessfullyChanged"] = true
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };
                controller.TempData = tempData;

                // Act
                var result = await controller.ChangePassword();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());

                var viewResult = result as ViewResult;
                Assert.That(viewResult.ViewName, Is.EqualTo("ChangePassword"));
                Assert.That(viewResult.Model, Is.InstanceOf<User>());

                var user = viewResult.Model as User;
                Assert.That(user.Login, Is.EqualTo("admin"));
                Assert.That(user.IsADIntegrated, Is.False);

                // Optional: ViewBag assertions
                Assert.That(controller.ViewBag.OldPasswordIsIncorrect, Is.True);
                Assert.That(controller.ViewBag.NewPasswordNotEqualToRepeat, Is.True);
                Assert.That(controller.ViewBag.DoesNotMeetComplexityRequirements, Is.True);
                Assert.That(controller.ViewBag.SucessfullyChanged, Is.True);
            }
        }

        [Test]
        public async Task ChangePassword_RedirectsToChangePassword_WhenSuccessful()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new AccountController(_logger, context);

                // Seed existing user (already seeded in [SetUp])
                var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Login == "admin");
                Assert.That(userInDb, Is.Not.Null);

                // Setup mock HttpContext with authenticated claims
                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", userInDb.Id.ToString()),
                        new Claim(ClaimTypes.Name, userInDb.Login)
                    }, "TestAuth"))
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };

                // Setup TempData
                controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

                // Simulate password change
                string oldPassword = "password";         // matches the one in Setup()
                string newPassword = "NewP@ssword123";   // passes policy
                string repeatPassword = "NewP@ssword123";

                // Bind only what's needed (Id + Login)
                var userInput = new User
                {
                    Id = userInDb.Id,
                    Login = userInDb.Login
                };

                // Act
                var result = await controller.ChangePassword(oldPassword, newPassword, repeatPassword, userInput);

                // Assert: redirected to ChangePassword
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("ChangePassword"));

                // Confirm password actually changed
                var updatedUser = await context.Users.FindAsync(userInDb.Id);
                Assert.That(updatedUser.Password, Is.EqualTo(newPassword));

                // Confirm audit log was written
                var auditLog = await context.AuditLogs.FirstOrDefaultAsync();
                Assert.That(auditLog, Is.Not.Null);
                Assert.That(auditLog.Description, Does.Contain("admin"));

                // Confirm TempData flag
                Assert.That(controller.TempData["SucessfullyChanged"], Is.True);
            }
        }


        #endregion
    }
}
