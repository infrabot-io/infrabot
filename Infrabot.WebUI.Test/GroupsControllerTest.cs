using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Infrabot.WebUI.Controllers;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Infrabot.WebUI.Test
{
    [TestFixture]
    public class GroupsControllerTest
    {
        private DbContextOptions<InfrabotContext> _options;
        private readonly ILogger<GroupsController> _logger = Mock.Of<ILogger<GroupsController>>();
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

            // Seed Groups
            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Group1",
                GroupPlugins = null,
                UserGroups = null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
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
        public async Task Index_ReturnsView_WithPaginatedGroups()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context); // Replace with your actual controller name

                // Setup mock authenticated HttpContext
                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", "1"),
                        new Claim(ClaimTypes.Name, "admin")
                    }, "TestAuth"))
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };

                // Add more than 1 group to test pagination logic
                for (int i = 2; i <= 55; i++)
                {
                    context.Groups.Add(new Group
                    {
                        Name = $"Group{i}",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }

                await context.SaveChangesAsync();

                // Act
                var result = await controller.Index(page: 0);

                // Assert: returns a ViewResult with group list
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult.Model, Is.InstanceOf<List<Group>>());

                var groups = viewResult.Model as List<Group>;
                Assert.That(groups.Count, Is.EqualTo(50)); // PageSize = 50

                // ViewBag assertions
                Assert.That(controller.ViewBag.Page, Is.EqualTo(0));
                Assert.That(controller.ViewBag.MaxPage, Is.EqualTo(1));
                Assert.That(controller.ViewBag.Pages, Is.EqualTo(2));
            }
        }

        #endregion


        #region Create Tests

        [Test]
        public async Task Create_Get_ReturnsViewWithTelegramUsers()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context);

                // Seed some TelegramUsers
                context.TelegramUsers.AddRange(
                    new TelegramUser { Id = 1, Name = "John", Surname = "Doe" },
                    new TelegramUser { Id = 2, Name = "Jane", Surname = "Smith" }
                );
                await context.SaveChangesAsync();

                var httpContext = new DefaultHttpContext();
                var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
                {
                    ["GroupAlreadyExists"] = true
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };
                controller.TempData = tempData;

                // Act
                var result = await controller.Create();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                var model = viewResult.Model as GroupViewModel;

                Assert.That(model, Is.Not.Null);
                Assert.That(model.AvailableTelegramUsers.Count, Is.EqualTo(2));
                Assert.That(controller.ViewBag.GroupAlreadyExists, Is.True);
            }
        }

        [Test]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValidAndGroupIsNew()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context);

                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("Login", "admin")
                    }, "TestAuth"))
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };
                controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

                // Seed TelegramUsers to assign to the group
                context.TelegramUsers.Add(new TelegramUser { Id = 100, Name = "TG", Surname = "User" });
                await context.SaveChangesAsync();

                var groupViewModel = new GroupViewModel
                {
                    Name = "NewGroup",
                    SelectedTelegramUserIds = new List<int> { 100 }
                };

                // Act
                var result = await controller.Create(groupViewModel);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirect = result as RedirectToActionResult;
                Assert.That(redirect.ActionName, Is.EqualTo("Index"));

                var groupInDb = await context.Groups.FirstOrDefaultAsync(g => g.Name == "NewGroup");
                Assert.That(groupInDb, Is.Not.Null);

                var userGroup = await context.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == groupInDb.Id && ug.TelegramUserId == 100);
                Assert.That(userGroup, Is.Not.Null);

                var audit = await context.AuditLogs.FirstOrDefaultAsync();
                Assert.That(audit, Is.Not.Null);
                Assert.That(audit.Description, Does.Contain("admin"));
            }
        }

        #endregion


        #region Edit Tests

        [Test]
        public async Task Edit_Get_ReturnsView_WhenGroupExists()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context);

                // Seed Telegram users and group with user assignments
                var user = new TelegramUser { Id = 999, Name = "TG", Surname = "User" };
                var group = new Group
                {
                    Name = "EditableGroup",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UserGroups = new List<UserGroup>
                    {
                        new UserGroup { TelegramUserId = 999 }
                    }
                };

                context.TelegramUsers.Add(user);
                context.Groups.Add(group);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.Edit(group.Id);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var view = result as ViewResult;
                var model = view.Model as GroupViewModel;

                Assert.That(model, Is.Not.Null);
                Assert.That(model.Name, Is.EqualTo("EditableGroup"));
                Assert.That(model.SelectedTelegramUserIds, Contains.Item(999));
                Assert.That(model.AvailableTelegramUsers.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Edit_Get_RedirectsToIndex_WhenGroupDoesNotExist()
        {
            using (var context = new InfrabotContext(_options))
            {
                var controller = new GroupsController(_logger, context);

                // Act
                var result = await controller.Edit(999); // ID that doesn't exist

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirect = result as RedirectToActionResult;
                Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            }
        }

        [Test]
        public async Task Edit_Post_UpdatesGroupAndRedirects_WhenValid()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context);

                // Seed Telegram user and group
                var user = new TelegramUser { Id = 88, Name = "TG", Surname = "Edit" };
                var group = new Group
                {
                    Name = "OldName",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                context.TelegramUsers.Add(user);
                context.Groups.Add(group);
                await context.SaveChangesAsync();

                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("Login", "admin")
                    }, "TestAuth"))
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };
                controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

                var model = new GroupViewModel
                {
                    Id = group.Id,
                    Name = "UpdatedName",
                    SelectedTelegramUserIds = new List<int> { 88 }
                };

                // Act
                var result = await controller.Edit(group.Id, model);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirect = result as RedirectToActionResult;
                Assert.That(redirect.ActionName, Is.EqualTo("Index"));

                var updated = await context.Groups.Include(g => g.UserGroups).FirstOrDefaultAsync(g => g.Id == group.Id);
                Assert.That(updated.Name, Is.EqualTo("UpdatedName"));
                Assert.That(updated.UserGroups.Any(ug => ug.TelegramUserId == 88), Is.True);

                var audit = await context.AuditLogs.FirstOrDefaultAsync();
                Assert.That(audit.Description, Does.Contain("UpdatedName"));
            }
        }

        [Test]
        public async Task Edit_Post_RedirectsToIndex_WhenGroupNotFound()
        {
            using (var context = new InfrabotContext(_options))
            {
                var controller = new GroupsController(_logger, context);

                var model = new GroupViewModel
                {
                    Id = 123,
                    Name = "GhostGroup",
                    SelectedTelegramUserIds = null
                };

                // Act
                var result = await controller.Edit(123, model);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirect = result as RedirectToActionResult;
                Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            }
        }

        #endregion


        #region Delete Tests

        [Test]
        public async Task Delete_Get_ReturnsView_WhenGroupExists()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context);

                var group = new Group
                {
                    Name = "GroupToDelete",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                context.Groups.Add(group);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.Delete(group.Id);

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult.Model, Is.TypeOf<Group>());

                var model = viewResult.Model as Group;
                Assert.That(model.Id, Is.EqualTo(group.Id));
                Assert.That(model.Name, Is.EqualTo("GroupToDelete"));
            }
        }

        [Test]
        public async Task DeletePressed_DeletesGroup_AndRedirectsToIndex()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                var controller = new GroupsController(_logger, context);

                var group = new Group
                {
                    Name = "GroupToRemove",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                context.Groups.Add(group);
                await context.SaveChangesAsync();

                // Setup HttpContext with login for audit log
                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("Login", "admin")
                    }, "TestAuth"))
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };

                controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

                // Act
                var result = await controller.DeletePressed(group.Id);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirect = result as RedirectToActionResult;
                Assert.That(redirect.ActionName, Is.EqualTo("Index"));

                var groupInDb = await context.Groups.FindAsync(group.Id);
                Assert.That(groupInDb, Is.Null);

                var audit = await context.AuditLogs.FirstOrDefaultAsync();
                Assert.That(audit, Is.Not.Null);
                Assert.That(audit.Description, Does.Contain("admin"));
                Assert.That(audit.Description, Does.Contain("GroupToRemove"));
            }
        }

        [Test]
        public async Task Delete_Get_RedirectsToIndex_WhenGroupNotFound()
        {
            using (var context = new InfrabotContext(_options))
            {
                var controller = new GroupsController(_logger, context);

                // Act
                var result = await controller.Delete(123); // Non-existent ID

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirect = result as RedirectToActionResult;
                Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            }
        }

        #endregion
    }
}
