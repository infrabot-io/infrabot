using Infrabot.Common.Contexts;
using Infrabot.WebUI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrabot.Common.Models;
using Infrabot.Common.Domain;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using Moq;

namespace Infrabot.WebUI.Test
{
    public class MessageStat
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }

    public class ApiControllerTest
    {
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
                var controller = new ApiController(_logger, _context);

                // Act: call the asynchronous Index action (default page = 0)
                Task<string> result = controller.GetResourceMetrics();

                // Assert: check that the result is a Task<string> using the constraint model
                Assert.That(result, Is.InstanceOf<Task<string>>());
                var data = await result;
                var healthCheck = JsonConvert.DeserializeObject< List<HealthCheck>>(data);
                var healthCheckItem = JsonConvert.DeserializeObject<HealthCheckItem>(healthCheck[0].Data);

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
                var controller = new ApiController(_logger, _context);

                // Act: call the asynchronous Index action (default page = 0)
                Task<string> result = controller.GetStats();

                // Assert: check that the result is a Task<string> using the constraint model
                Assert.That(result, Is.InstanceOf<Task<string>>());
                var data = await result;
                var statsItem = JsonConvert.DeserializeObject<StatsItem>(data);

                Assert.That(statsItem.TelegramUsers, Is.EqualTo(1));
                Assert.That(statsItem.Plugins, Is.EqualTo(1));
                Assert.That(statsItem.Users, Is.EqualTo(0));
                Assert.That(statsItem.StatsEvents[0].EventType, Is.EqualTo(Common.Enums.EventLogType.Info));
            }
        }

        [Test]
        public async Task GetMessageStats_ReturnsStats()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ApiController(_logger, _context);

                // Act: call the asynchronous Index action (default page = 0)
                Task<string> result = controller.GetMessageStats();

                // Assert: check that the result is a Task<string> using the constraint model
                Assert.That(result, Is.InstanceOf<Task<string>>());
                var data = await result;
                Console.WriteLine(data);
                var messageStats = JsonConvert.DeserializeObject<List<MessageStat>>(data);
                
                Assert.That(messageStats.Where(x => x.Count == 1).FirstOrDefault().Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task GetPluginStats_ReturnsStats()
        {
            // Arrange: create a new context instance for testing
            using (var _context = new InfrabotContext(_options))
            {
                var controller = new ApiController(_logger, _context);

                // Act: call the asynchronous Index action (default page = 0)
                Task<string> result = controller.GetPluginStats();

                // Assert: check that the result is a Task<string> using the constraint model
                Assert.That(result, Is.InstanceOf<Task<string>>());
                var data = await result;
                var plugins = JsonConvert.DeserializeObject<List<Plugin>>(data);
                
                Assert.That(plugins.Count, Is.EqualTo(1));
            }
        }
    }
}
