using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Infrabot.Common.Contexts;
using Microsoft.EntityFrameworkCore;
using Infrabot.TelegramService.Managers;
using Infrabot.TelegramService.Core;

namespace Infrabot.TelegramService.Services
{
    public class TelegramService : BackgroundService
    {
        private readonly ILogger<TelegramService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private IPluginRegistry _pluginRegistry;
        private CommandManager _commandManager;

        // Telegram configuration
        private TelegramBotClient _botClient;

        public TelegramService(
            ILogger<TelegramService> logger, 
            ILoggerFactory loggerFactory,
            IPluginRegistry pluginRegistry,
            IServiceScopeFactory scopeFactory
        )
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _scopeFactory = scopeFactory;
            _pluginRegistry = pluginRegistry;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            _logger.LogInformation("Init: Reading configuration from the database");
            var configuration = await _context.Configurations.Where(x => x.Id == 1).FirstOrDefaultAsync();
            if (configuration is null)
            {
                _logger.LogError("Configuration is null. Stopping service. Fix system confiruation first!");
                Environment.Exit(1);
            }

            try
            {
                _logger.LogInformation("Starting Telegram Bot service...");
                _botClient = new TelegramBotClient(configuration.TelegramBotToken);
            }
            catch (Exception ex) 
            {
                _logger.LogError("Could not init Telegram Client: " + ex.Message);
                Environment.Exit(1);
            }

            _logger.LogInformation("Telegram Bot service successfully started");
            var me = await _botClient.GetMe(stoppingToken);
            _logger.LogInformation($"Telegram bot @{me.Username} is running.");

            // Init command manager after bot client loaded
            _logger.LogInformation($"Init: EmergencyStateManager");
            var emergencyStateManager = scope.ServiceProvider.GetRequiredService<IEmergencyStateManager>();
            _logger.LogInformation($"Init: TelegramResponder");
            var telegramResponder = new TelegramResponder(_botClient, _loggerFactory.CreateLogger<TelegramResponder>());
            _logger.LogInformation($"Init: CommandHandlerFactory");
            var _commandHandlerFactory = new CommandHandlerFactory(_botClient, configuration, _pluginRegistry, emergencyStateManager, telegramResponder, _scopeFactory);
            _logger.LogInformation($"Init: CommandManager"); 
            _commandManager = new CommandManager(
                _loggerFactory.CreateLogger<CommandManager>(),
                _commandHandlerFactory
            );

            _logger.LogInformation($"Init: Begin receiving telegram messages");
            // Begin receiving messages
            _botClient.OnMessage += OnMessage;

            try
            {
                // Wait for cancellation
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Poll for cancellation
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Telegram Bot service cancellation requested. Stopping service.");
            }
            finally
            {
                // Stop receiving and cleanup
                // Unregister event handlers
                _botClient.OnMessage -= OnMessage;
                _logger.LogInformation("Telegram Bot service stopped.");
            }
        }

        public async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text is null) return;

            // Log telegram message for debug purposes. Must set MinimumLevel to Debug in the application.json file
            _logger.LogDebug($"Message ({message.From?.Username} - {message.From?.Id}): " + message.Text);
            
            // Use this construction to truly run Async because of CliWrap library
            // If we simply run await HandleCommand, then commands will handle synchronously
            // If some command gets stuck (until its timeout reached) we will have to wait it to finish 
            // until next command is executed which we do not want
            _ = Task.Run(async () =>
            {
                await _commandManager.HandleCommand(message);
            });
        }
    }
}
