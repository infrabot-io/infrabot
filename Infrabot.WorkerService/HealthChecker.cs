using Infrabot.Common.Contexts;
using Infrabot.Common.Domain;
using Infrabot.Common.Models;
using Infrabot.WorkerService.Utils;
using Newtonsoft.Json;

namespace Infrabot.WorkerService
{
    public class HealthChecker : BackgroundService
    {
        private readonly ILogger<HealthChecker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _period;
        private readonly TimeSpan _cacheValidity = TimeSpan.FromMinutes(5);

        private DateTime _lastPollTime;
        private HealthCheckItem _cachedHealthCheckItem;

        public HealthChecker(ILogger<HealthChecker> logger,  IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _lastPollTime = DateTime.MinValue;
            _cachedHealthCheckItem = null;
            _configuration = configuration;

            _logger.LogInformation("Init: Health checker service");

            int period = Convert.ToInt32(_configuration["Services:HealthCheckerIntervalMinutes"]);
            _logger.LogInformation($"Health Checker interval is set to {period} minutes.");
            _period = TimeSpan.FromMinutes(period);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Executing CheckHealth");
                await CheckHealth();
                _logger.LogInformation("Execution of CheckHealth finished");
            }
        }

        public async Task CheckHealth()
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            try
            {
                HealthCheckItem healthCheckItem = GetHealthCheckItem();

                _context.HealthChecks.Add(
                    new HealthCheck
                    {
                        Data = JsonConvert.SerializeObject(healthCheckItem),
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    }
                );

                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Execution of CheckHealth failed. Error: {ex.Message}");
            }
        }

        private HealthCheckItem GetHealthCheckItem()
        {
            DateTime now = DateTime.UtcNow;
            if (_cachedHealthCheckItem != null && (now - _lastPollTime) < _cacheValidity)
            {
                return _cachedHealthCheckItem; // Return cached data if still valid
            }

            // Query hardware stats if cache is invalid or empty
            HardwareInfo.GetRamGB(out ulong availableRam, out ulong totalRam);
            int ramUsage = Convert.ToInt32(totalRam - availableRam);
            int cpuUsage = HardwareInfo.GetCPUUsage();

            _cachedHealthCheckItem = new HealthCheckItem
            {
                CpuUsage = cpuUsage,
                RamUsage = ramUsage
            };

            _lastPollTime = now;
            return _cachedHealthCheckItem;
        }
    }
}
