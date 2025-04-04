using Infrabot.Common.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Infrabot.WebUI.Controllers;
using Microsoft.Extensions.Logging;
using Infrabot.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrabot.WebUI.Test
{
    [TestFixture]
    public class LogsControllerTest
    {
        private DbContextOptions<InfrabotContext> _options;
        private readonly ILogger<LogsController> _logger = Mock.Of<ILogger<LogsController>>();
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
        public async Task Index_SetsViewBagLogs_WhenLogFileExists()
        {
            using (var context = new InfrabotContext(_options))
            {
                // Arrange
                string tempLogFile = Path.GetTempFileName();

                try
                {
                    // Write dummy log lines
                    File.WriteAllLines(tempLogFile, Enumerable.Range(1, 600).Select(i => $"Log line {i}"));

                    var controller = new LogsController(_logger, context);

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

                    // Manually set logsFilePath (make sure controller supports it)
                    typeof(LogsController)
                        .GetField("logsFilePath", BindingFlags.NonPublic | BindingFlags.Instance)!
                        .SetValue(controller, tempLogFile);

                    // Act
                    var result = await controller.Index();

                    // Assert
                    Assert.That(result, Is.InstanceOf<ViewResult>());
                    var logs = controller.ViewBag.Logs as string;

                    Assert.That(logs, Is.Not.Null);
                    var logLines = logs.Split(Environment.NewLine);
                    Assert.That(logLines.Length, Is.EqualTo(500));
                    Assert.That(logLines.First(), Is.EqualTo("Log line 101")); // First of the last 500
                    Assert.That(logLines.Last(), Is.EqualTo("Log line 600")); // Last line
                }
                finally
                {
                    File.Delete(tempLogFile);
                }
            }
        }

        [Test]
        public async Task Index_SetsViewBagLogs_Empty_WhenFileDoesNotExist()
        {
            using (var context = new InfrabotContext(_options))
            {
                var controller = new LogsController(_logger, context);

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

                typeof(LogsController)
                    .GetField("logsFilePath", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(controller, "nonexistent.log");

                var result = await controller.Index();

                Assert.That(result, Is.InstanceOf<ViewResult>());
                Assert.That(controller.ViewBag.Logs, Is.EqualTo(string.Empty));
            }
        }

        #endregion
    }
}
