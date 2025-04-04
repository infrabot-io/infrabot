using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Infrabot.WebUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Infrabot.WebUI.Test
{
    [TestFixture]
    public class ConfigurationControllerTest
    {
        private DbContextOptions<InfrabotContext> _options;
        private readonly ILogger<ConfigurationController> _logger = Mock.Of<ILogger<ConfigurationController>>();
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
        public async Task Index_ReturnsView()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ConfigurationController(_logger, _context);

                // Act
                var result = await controller.Index();

                // Assert
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult, Is.Not.Null);
            }
        }

        [Test]
        public async Task Index_RedirectsToIndex_WhenModelIsValid()
        {
            using (var _context = new InfrabotContext(_options))
            {
                // Arrange controller
                var controller = new ConfigurationController(_logger, _context);

                // Setup fake HttpContext with authenticated user
                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("Login", "admin"),
                        new Claim(ClaimTypes.Name, "password")
                    }, "TestAuth"))
                };

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };

                // Fetch config from DB and change a value
                var configuration = await _context.Configurations.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.That(configuration, Is.Not.Null);
                configuration.TelegramResultMaxLength = 6000;

                // Ensure ModelState is valid (simulate successful model binding)
                controller.ModelState.Clear();

                // Act
                var result = await controller.Index(configuration);

                // Assert
                Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
                var redirectResult = result as RedirectToActionResult;
                Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

                var updated = await _context.Configurations.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.That(updated.TelegramResultMaxLength, Is.EqualTo(6000));

                var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
                Assert.That(auditLog, Is.Not.Null);
                Assert.That(auditLog.Description, Does.Contain("admin"));
            }
        }

        #endregion

    }
}
