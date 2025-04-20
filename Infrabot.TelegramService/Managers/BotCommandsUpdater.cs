using Infrabot.TelegramService.Core;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Managers
{
    public class BotCommandsUpdater : IBotCommandsUpdater
    {
        private readonly ILogger<BotCommandsUpdater> _logger;
        private IPluginRegistry _pluginRegistry;
        private ITelegramBotClient _botClient;
        private readonly TimeSpan _period;

        public BotCommandsUpdater(
            ILogger<BotCommandsUpdater> logger,
            IPluginRegistry pluginRegistry, 
            ITelegramBotClient botClient)
        {
            _logger = logger;
            _pluginRegistry = pluginRegistry;
            _botClient = botClient;

            _logger.LogInformation("Init: Bot Commands Updater service");

            _logger.LogInformation($"Bot Commans Updater interval is set to 5 hours.");
            _period = TimeSpan.FromHours(1);

            CancellationToken stoppingToken = new CancellationToken();
            ExecuteAsync(stoppingToken);
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Executing Bot Commands Updater");
                await UpdateCommands();
                _logger.LogInformation("Execution of Bot Commands Updater finished");
            }
        }

        public async Task UpdateCommands()
        {
            try
            {
                List<BotCommand> botCommands = new List<BotCommand>();

                botCommands.Add(new BotCommand()
                {
                    Command = "/reloadplugins",
                    Description = "Force reload plugins."
                });

                botCommands.Add(new BotCommand()
                {
                    Command = "/emergency",
                    Description = "Force shuts down service."
                });

                botCommands.Add(new BotCommand()
                {
                    Command = "/showmyid",
                    Description = "Shows user id"
                });

                botCommands.Add(new BotCommand()
                {
                    Command = "/getcommands",
                    Description = "Gets a list of all available commands"
                });

                if(_pluginRegistry.Plugins.Count > 0)
                {
                    foreach(var plugin in _pluginRegistry.Plugins)
                    {
                        if(plugin.PluginExecutions.Count > 0)
                        {
                            foreach (var execution in plugin.PluginExecutions)
                            {
                                botCommands.Add(new BotCommand()
                                {
                                    Command = $"{execution.CommandName}",
                                    Description = $"{execution.Help}"
                                });
                            }
                        }
                    }

                    await _botClient.SetMyCommands(botCommands);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Execution of  Bot Commands Updater failed. Error: {ex.Message}");
            }
        }
    }
}
