using Infrabot.Common.Models;
using Infrabot.TelegramService.Commands;
using Infrabot.TelegramService.Core;
using Telegram.Bot;

namespace Infrabot.TelegramService.Managers
{
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Configuration _configuration;
        private readonly IPluginRegistry _pluginRegistry;
        private readonly IEmergencyStateManager _emergencyStateManager; 
        private readonly ITelegramResponder _telegramResponder;
        private readonly IBotCommandsUpdater _botCommandsUpdater;
        private readonly IServiceScopeFactory _scopeFactory;

        public CommandHandlerFactory(
            ITelegramBotClient botClient,
            Configuration configuration,
            IPluginRegistry pluginRegistry,
            IEmergencyStateManager emergencyStateManager,
            ITelegramResponder telegramResponder,
            IBotCommandsUpdater botCommandsUpdater,
            IServiceScopeFactory scopeFactory)
        {
            _botClient = botClient;
            _configuration = configuration;
            _pluginRegistry = pluginRegistry;
            _emergencyStateManager = emergencyStateManager;
            _telegramResponder = telegramResponder;
            _botCommandsUpdater = botCommandsUpdater;
            _scopeFactory = scopeFactory;
        }

        public List<ICommandHandler> GetBuiltInCommands()
        {
            var list = new List<ICommandHandler>();

            if (_configuration.TelegramEnableShowMyId)
                list.Add(new ShowMyIdCommand(_telegramResponder));

            if (_configuration.TelegramEnableEmergency)
                list.Add(new EmergencyCommand(_telegramResponder, _emergencyStateManager));

            list.Add(new GetCommandsCommand(_telegramResponder, _pluginRegistry));

            list.Add(new ReloadPluginsCommand(_telegramResponder, _pluginRegistry));

            return list;
        }

        public ITelegramBotClient GetBotClient()
        {
            return _botClient;
        }

        public IPluginRegistry GetPluginRegistry()
        {
            return _pluginRegistry;
        }

        public Configuration GetConfiguration()
        {
            return _configuration;
        }

        public ITelegramResponder GetTelegramResponder()
        {
            return _telegramResponder;
        }

        public IBotCommandsUpdater GetBotCommandsUpdater()
        {
            return _botCommandsUpdater;
        }

        public IServiceScopeFactory GetServiceScopeFactory()
        {
            return _scopeFactory;
        }
    }
}
