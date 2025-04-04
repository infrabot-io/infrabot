using Infrabot.Common.Contexts;
using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WorkerService
{
    public class HealthDataCleaner : BackgroundService
    {
        private readonly ILogger<HealthDataCleaner> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _period;

        public HealthDataCleaner(ILogger<HealthDataCleaner> logger,  IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
            _configuration = configuration;

            _logger.LogInformation("Init: Health Data Cleaner service");

            int period = Convert.ToInt32(_configuration["Services:HealthDataCleanerIntervalMinutes"]);
            _logger.LogInformation($"Health Data Cleaner interval is set to {period} minutes.");
            _period = TimeSpan.FromMinutes(period);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Executing CleanHealthData");
                await CleanHealthData();
                _logger.LogInformation("Execution of CleanHealthData finished");
            }
        }

        public async Task CleanHealthData()
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            try
            {
                // Messages to delete less than specified days
                int keepHealthChecksDays = Convert.ToInt32(_configuration["Services:HealthDataCleanerKeepLastDays"]);

                // Cleanup old data
                var oldHealthChecks = await _context.HealthChecks.Where(x => x.CreatedDate < DateTime.UtcNow.AddDays(-keepHealthChecksDays)).ToListAsync();
                if (oldHealthChecks.Any())
                {
                    _context.HealthChecks.RemoveRange(oldHealthChecks);
                    _context.EventLogs.Add(new EventLog { EventType = EventLogType.Info, CreatedDate = DateTime.Now, Description = $"CleanHealthData. Cleaned health check data older than {keepHealthChecksDays} days. Cleaned data count: {oldHealthChecks.Count}." });
                }
                else
                {
                    _context.EventLogs.Add(new EventLog { EventType = EventLogType.Info, CreatedDate = DateTime.Now, Description = $"CleanHealthData. No health check data to cleanup detected. Waiting for the next schedule." });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Execution of CleanHealthData failed. Error: {ex.Message}");
            }
        }
    }
}
