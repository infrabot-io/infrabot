using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Infrabot.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WorkerService
{
    public class MessageCleaner : BackgroundService
    {
        private readonly ILogger<MessageCleaner> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _period;

        public MessageCleaner(ILogger<MessageCleaner> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
            _configuration = configuration;

            _logger.LogInformation("Init: Message Cleaner service");

            int period = Convert.ToInt32(_configuration["Services:MessageCleanerIntervalMinutes"]);
            _logger.LogInformation($"Message Cleaner interval is set to {period} minutes.");
            _period = TimeSpan.FromMinutes(period);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Executing CleanMessages");
                await CleanMessages();
                _logger.LogInformation("Execution of CleanMessages finished");
            }
        }

        public async Task CleanMessages()
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            try
            {
                // Messages to delete less than specified days
                int keepTelegramMessagesDays = Convert.ToInt32(_configuration["Services:MessageCleanerKeepLastDays"]);

                // Cleanup old data
                var oldTelegramMessages = await _context.TelegramMessages.Where(x => x.CreatedDate < DateTime.UtcNow.AddDays(-keepTelegramMessagesDays)).ToListAsync();
                if (oldTelegramMessages.Any())
                {
                    _context.TelegramMessages.RemoveRange(oldTelegramMessages);
                    _context.EventLogs.Add(new EventLog { EventType = EventLogType.Info, CreatedDate = DateTime.Now, Description = $"MessageCleaner. Cleaned telegram messages older than {keepTelegramMessagesDays} days. Cleaned messages count: {oldTelegramMessages.Count}." });
                }
                else
                {
                    _context.EventLogs.Add(new EventLog { EventType = EventLogType.Info, CreatedDate = DateTime.Now, Description = $"MessageCleaner. No messages to cleanup detected. Waiting for the next schedule." });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Execution of CleanMessages failed. Error: {ex.Message}");
            }
        }
    }
}
