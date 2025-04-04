using Microsoft.EntityFrameworkCore;
using Infrabot.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Infrabot.WebUI.Controllers;
using infrabot.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using Infrabot.Common.Enums;
using Infrabot.Common.Contexts;
using Microsoft.Data.Sqlite;

namespace Infrabot.WebUI.Test
{
    [TestFixture]
    public class UsersControllerTest
    {
        private DbContextOptions<InfrabotContext> _options;
        private readonly ILogger<HomeController> _logger = Mock.Of<ILogger<HomeController>>(); 
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

        #region Index Tests

        [Test]
        public async Task Index_ReturnsViewResult_WithUsers()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act: call the asynchronous Index action (default page = 0)
                var result = await controller.Index();

                // Assert: check that the result is a ViewResult using the constraint model
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);

                // Assert: verify that the model is an List<User>>
                Assert.That(viewResult.ViewData.Model, Is.InstanceOf<List<User>>());
                var model = viewResult.ViewData.Model as List<User>;
                Assert.That(model.Count(), Is.EqualTo(1));
            }
        }

        #endregion

        #region Create Action Tests

        [Test]
        public void Create_Get_ReturnsView_WithTempDataInViewBag()
        {
            // Arrange
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Set up TempData using a fake provider.
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                controller.TempData["ConfigurationError"] = "Error1";
                controller.TempData["UserAlreadyExists"] = "Error2";
                controller.TempData["PasswordDoesNotMeetComplexity"] = "Error3";

                // Act
                var result = controller.Create();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);

                // Check values stored in ViewData (the equivalent of ViewBag)
                Assert.That(viewResult.ViewData["ConfigurationError"], Is.EqualTo("Error1"));
                Assert.That(viewResult.ViewData["UserAlreadyExists"], Is.EqualTo("Error2"));
                Assert.That(viewResult.ViewData["PasswordDoesNotMeetComplexity"], Is.EqualTo("Error3"));
            }
        }

        [Test]
        public async Task Create_Post_NullConfiguration_RedirectsToCreateWithSystemConfigurationError()
        {
            // Arrange: use a fresh context with no Configuration record.
            using(var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                // Clean configuration
                var configuration = await _context.Configurations.Where(x => x.Id == 1).FirstOrDefaultAsync();
                _context.Configurations.Remove(configuration);
                await _context.SaveChangesAsync();

                // Create a sample user.
                var user = new User
                {
                    Id = 2,
                    Name = "Bad",
                    Surname = "User",
                    Login = "admin2",
                    Email = "",
                    Password = "password123",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Phone = "",
                    IsADIntegrated = false,
                    Enabled = true
                };

                // Act
                var result = await controller.Create(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = (RedirectToActionResult)result;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Create"));
                Assert.That(controller.TempData["SystemConfigurationError"], Is.EqualTo(true));
            }
        }

        [Test]
        public async Task Create_Post_UserAlreadyExists_RedirectsToCreateWithUserAlreadyExists()
        {
            // Arrange: create a context with a valid configuration and an existing user.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                // Create a new user with same login (or email).
                var user = new User
                {
                    Id = 2,
                    Name = "Bad",
                    Surname = "User",
                    Login = "admin",
                    Email = "",
                    Password = "password123",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Phone = "",
                    IsADIntegrated = false,
                    Enabled = true
                };

                // Act
                var result = await controller.Create(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = (RedirectToActionResult)result;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Create"));
                Assert.That(controller.TempData["UserAlreadyExists"], Is.EqualTo(true));
            }
        }

        [Test]
        public async Task Create_Post_PasswordDoesNotMeetComplexity_RedirectsToCreateWithPasswordError()
        {
            // Arrange: context with valid configuration and no conflicting user.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                // Create a user with a weak password.
                var user = new User
                {
                    Id = 2,
                    Name = "Bad",
                    Surname = "User",
                    Login = "admin2",
                    Email = "",
                    Password = "pass",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Phone = "",
                    IsADIntegrated = false,
                    Enabled = true
                };

                // Act
                var result = await controller.Create(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = (RedirectToActionResult)result;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Create"));
                Assert.That(controller.TempData["PasswordDoesNotMeetComplexity"], Is.EqualTo(true));
            }
        }

        [Test]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithUserModel()
        {
            // Arrange: valid configuration and no user conflict, but simulate ModelState error.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
                
                // Simulate a ModelState error.
                controller.ModelState.AddModelError("Test", "Error");

                var user = new User
                {
                    Id = 2,
                    Name = "Valid",
                    Surname = "User",
                    Login = "admin2",
                    Email = "",
                    Password = "password123",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Phone = "",
                    IsADIntegrated = false,
                    Enabled = true
                };

                // Act
                var result = await controller.Create(user);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = (ViewResult)result;
                Assert.That(viewResult.Model, Is.EqualTo(user));
            }
        }

        [Test]
        public async Task Create_Post_ValidUser_RedirectsToIndexAndCreatesUserAndAuditLog()
        {
            // Arrange: valid configuration with relaxed complexity and no existing user.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);
                controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

                // Setup HttpContext with a fake user claim "Login" so that AuditLog creation works.
                var httpContext = new DefaultHttpContext();
                var claims = new List<Claim> { new Claim("Login", "admin") };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                // Create a valid user.
                var user = new User
                {
                    Id = 2,
                    Name = "Valid",
                    Surname = "User",
                    Login = "admin2",
                    Email = "",
                    Password = "password123",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Phone = "",
                    IsADIntegrated = false,
                    Enabled = true
                };

                // Act
                var result = await controller.Create(user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = (RedirectToActionResult)result;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

                // Verify that the user was added to the database.
                Assert.That(_context.Users.Any(u => u.Login == "admin2"), Is.True);
                // Verify that an audit log entry was created.
                Assert.That(_context.AuditLogs.Any(a => a.Description.Contains("created user 'admin2'")), Is.True);
            }
        }

        #endregion

        #region Edit Action Tests

        [Test]
        public async Task Edit_Get_ReturnsView_WithUserModel_WhenUserExists()
        {
            // Arrange: add a user with Id = 1.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act
                var result = await controller.Edit(1);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult.Model, Is.InstanceOf<User>());
                var model = viewResult.Model as User;
                Assert.That(model.Id, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Edit_Get_RedirectsToIndex_WhenUserDoesNotExist()
        {
            // Arrange: no user with Id = 1 in the database.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act
                var result = await controller.Edit(2);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            }
        }

        [Test]
        public async Task Edit_Post_RedirectsToIndex_WhenUserNotFound()
        {
            // Arrange: no user with the given Id exists.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                var user =  await _context.Users.Where(x => x.Id == 2).FirstOrDefaultAsync();

                // Act
                var result = await controller.Edit(2, user);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            }
        }

        [Test]
        public async Task Edit_Post_ReturnsView_WithUserModel_WhenModelStateIsInvalid()
        {
            // Arrange: add an existing user in the database.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Simulate a ModelState error.
                controller.ModelState.AddModelError("Test", "Error");

                // Prepare updated user data.
                var updatedUser = new User
                {
                    Id = 1,
                    Login = "admin",
                    Email = "newemail@example.com",
                    Name = "NewName",
                    Surname = "NewSurname"
                };

                // Act
                var result = await controller.Edit(1, updatedUser);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult.Model, Is.EqualTo(updatedUser));
            }
        }

        [Test]
        public async Task Edit_Post_ValidUser_UpdatesUserAndRedirectsToIndex()
        {
            // Arrange: add an existing user to the database.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Set up HttpContext with a fake user claim "Login" for audit log creation.
                var httpContext = new DefaultHttpContext();
                var claims = new List<Claim> { new Claim("Login", "admin") };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                // Prepare updated user data.
                var updatedUser = new User
                {
                    Id = 1,
                    Login = "user1_updated",
                    Email = "user1_updated@example.com",
                    Name = "NewName",
                    Surname = "NewSurname",
                    IsADIntegrated = true,
                    Enabled = true,
                    Phone = "222222",
                    Password = "newpassword"
                };

                // Act
                var result = await controller.Edit(1, updatedUser);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

                // Verify that the user record was updated.
                var userInDb = _context.Users.FirstOrDefault(u => u.Id == 1);
                Assert.That(userInDb, Is.Not.Null);
                Assert.That(userInDb.Login, Is.EqualTo("user1_updated"));
                Assert.That(userInDb.Email, Is.EqualTo("user1_updated@example.com"));
                Assert.That(userInDb.Name, Is.EqualTo("NewName"));
                Assert.That(userInDb.Surname, Is.EqualTo("NewSurname"));
                Assert.That(userInDb.IsADIntegrated, Is.True);
                Assert.That(userInDb.Enabled, Is.True);
                Assert.That(userInDb.Phone, Is.EqualTo("222222"));
                Assert.That(userInDb.Password, Is.EqualTo("newpassword"));

                // Verify that an audit log entry was created.
                var auditLog = _context.AuditLogs.FirstOrDefault(a => a.Description.Contains("updated user 'user1_updated'"));
                Assert.That(auditLog, Is.Not.Null);
                Assert.That(auditLog.LogAction, Is.EqualTo(AuditLogAction.Update));
                Assert.That(auditLog.LogItem, Is.EqualTo(AuditLogItem.User));
            }
        }

        #endregion

        #region Delete Action Tests

        [Test]
        public async Task Delete_Get_ReturnsView_WithUserModel_WhenUserExists()
        {
            // Arrange
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act
                var result = await controller.Delete(1);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult.Model, Is.InstanceOf<User>());
                var model = viewResult.Model as User;
                Assert.That(model.Id, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Delete_Get_RedirectsToIndex_WhenUserDoesNotExist()
        {
            // Arrange
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act
                var result = await controller.Delete(2);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            }
        }

        [Test]
        public async Task DeletePressed_Post_DeletesUserAndAddsAuditLog_WhenUserExists()
        {
            // Arrange
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Set up HttpContext with a fake user claim "Login" for audit log purposes.
                var httpContext = new DefaultHttpContext();
                var claims = new List<Claim> { new Claim("Login", "admin") };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                // Act
                var result = await controller.DeletePressed(1);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

                // Verify that the user was removed.
                Assert.That(_context.Users.FirstOrDefault(u => u.Id == 1), Is.Null);
                // Verify that an audit log entry was created.
                Assert.That(_context.AuditLogs.Any(a => a.Description.Contains("deleted user 'admin'")), Is.True);
            }
        }

        [Test]
        public async Task DeletePressed_Post_RedirectsToIndex_WhenUserDoesNotExist()
        {
            // Arrange
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Set up HttpContext with a fake claim.
                var httpContext = new DefaultHttpContext();
                var claims = new List<Claim> { new Claim("Login", "admin") };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                // Act
                var result = await controller.DeletePressed(2);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            }
        }

        #endregion

        #region View Action Tests

        [Test]
        public async Task View_ReturnsView_WithUserModel_WhenUserExists()
        {
            // Arrange: Seed the in-memory database with a test user.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act
                var result = await controller.View(1);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult.Model, Is.InstanceOf<User>());
                var model = viewResult.Model as User;
                Assert.That(model.Id, Is.EqualTo(1));
                Assert.That(model.Login, Is.EqualTo("admin"));
            }
        }

        [Test]
        public async Task View_RedirectsToIndex_WhenUserDoesNotExist()
        {
            // Arrange: Create a context without seeding any user.
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new UsersController(_logger, _context);

                // Act
                var result = await controller.View(2);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            }
        }

        #endregion
    }
}