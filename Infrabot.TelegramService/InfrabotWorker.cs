using Infrabot.Common.Contexts;

namespace Infrabot.TelegramService
{
    public class InfrabotWorker : BackgroundService
    {
        private readonly ILogger<InfrabotWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public InfrabotWorker(ILogger<InfrabotWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                
                var aa = _context.Users.Where(x => x.Id == 1).FirstOrDefault();
                Console.WriteLine(aa.Login);

                await Task.Delay(50000, stoppingToken);
            }
        }
    }
}
