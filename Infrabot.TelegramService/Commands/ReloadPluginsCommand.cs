using Infrabot.TelegramService.Core;
using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Commands
{
    public class ReloadPluginsCommand : ICommandHandler
    {
        private readonly ITelegramResponder _telegramResponder;
        private readonly IPluginRegistry _pluginRegistry;
        public string Command => "/reloadplugins";

        public ReloadPluginsCommand(ITelegramResponder telegramResponder, IPluginRegistry pluginRegistry)
        {
            _telegramResponder = telegramResponder;
            _pluginRegistry = pluginRegistry;
        }

        public async Task ExecuteAsync(Message message)
        {
            _pluginRegistry.RefreshPlugins();
            await _telegramResponder.SendMarkdown(message.Chat, "✅ Plugins successfully reloaded");
        }
    }
}
