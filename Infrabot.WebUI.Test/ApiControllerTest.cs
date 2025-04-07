using Infrabot.Common.Contexts;
using Infrabot.WebUI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrabot.Common.Models;
using Infrabot.Common.Domain;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Test
{
    public class ApiControllerTest
    {
        /*
        private DbContextOptions<InfrabotContext> _options;
        private readonly ILogger<ApiController> _logger = Mock.Of<ILogger<ApiController>>();
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {
            // Use SQLite in-memory with persistent connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open(); // MUST stay open for the life of the context

            _options = new DbContextOptionsBuilder<InfrabotContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new InfrabotContext(_options);
            context.Database.EnsureCreated(); // Required for SQLite in-memory DB

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

            HealthCheckItem healthCheckItem = new HealthCheckItem
            {
                CpuUsage = 10,
                RamUsage = 4
            };

            context.HealthChecks.Add(new HealthCheck
            {
                Id = 1,
                Data = JsonConvert.SerializeObject(healthCheckItem),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            context.EventLogs.Add(new EventLog
            {
                Id = 1,
                EventType = Common.Enums.EventLogType.Info,
                Description = "empty",
                CreatedDate = DateTime.UtcNow
            });

            context.TelegramUsers.Add(new TelegramUser
            {
                Id = 1,
                Name = "Test",
                Surname = "User",
                TelegramId = 123,
                UserGroups = null,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
            });

            context.TelegramMessages.Add(new TelegramMessage
            {
                Id = 1,
                Message = "test",
                TelegramUserId = 123,
                TelegramUserUsername = "test",
                CreatedDate = DateTime.UtcNow
            });

            context.Plugins.Add(new Plugin
            {
                Id = 1,
                Name = "Test1",
                Guid = Guid.NewGuid(),
                PermissionAssignments = null,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        [Test]
        public async Task GetResourceMetrics_ReturnsMetrics()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ApiController(_logger, new ApiService(_context));

                // Act
                var actionResult = await controller.GetResourceMetrics();
                var okResult = actionResult as OkObjectResult;
                var jsonString = okResult.Value as string;

                // Deserialize
                var healthChecks = JsonConvert.DeserializeObject<List<HealthCheck>>(jsonString);
                var healthCheckItem = JsonConvert.DeserializeObject<HealthCheckItem>(healthChecks[0].Data);

                // Assert
                Assert.That(healthCheckItem.CpuUsage, Is.EqualTo(10));
                Assert.That(healthCheckItem.RamUsage, Is.EqualTo(4));
            }
        }

        [Test]
        public async Task GetStats_ReturnsStats()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ApiController(_logger, new ApiService(_context));

                // Act
                var actionResult = await controller.GetStats();
                var okResult = actionResult as OkObjectResult;
                var jsonString = okResult.Value as string;
                var statsItem = JsonConvert.DeserializeObject<StatsItem>(jsonString);

                // Assert
                Assert.That(statsItem.TelegramUsers, Is.EqualTo(1));
                Assert.That(statsItem.Plugins, Is.EqualTo(1));
                Assert.That(statsItem.Users, Is.EqualTo(0));
                Assert.That(statsItem.StatsEvents.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(statsItem.StatsEvents[0].EventType, Is.EqualTo(Common.Enums.EventLogType.Info));
            }
        }

        [Test]
        public async Task GetMessageStats_ReturnsStats()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ApiController(_logger, new ApiService(_context));

                // Act
                var actionResult = await controller.GetMessageStats();
                var okResult = actionResult as OkObjectResult;
                var jsonString = okResult.Value as string;
                var messageStats = JsonConvert.DeserializeObject<List<MessageStat>>(jsonString);

                // Assert: verify that there's a message stat with Count equal to 1.
                var messageStat = messageStats.FirstOrDefault(ms => ms.Count == 1);
                Assert.That(messageStat.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task GetPluginStats_ReturnsStats()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ApiController(_logger, new ApiService(_context));

                // Act
                var actionResult = await controller.GetPluginStats();
                var okResult = actionResult as OkObjectResult;
                var jsonString = okResult.Value as string;
                var pluginStats = JsonConvert.DeserializeObject<List<PluginStat>>(jsonString);

                // Assert: verify that one plugin stat is returned.
                Assert.That(pluginStats.Count, Is.EqualTo(1));
                Assert.That(pluginStats[0].PluginType, Is.EqualTo("Test1"));
                Assert.That(pluginStats[0].Count, Is.EqualTo(1));
            }
        }
        */
    }
}
