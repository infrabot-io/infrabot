using Infrabot.PluginSystem;
using Infrabot.PluginSystem.Execution;
using Infrabot.TelegramService.Core;
using System.Text;
using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Commands
{
    public class GetCommandsCommand : ICommandHandler
    {
        private readonly ITelegramResponder _telegramResponder;
        private readonly IPluginRegistry _pluginRegistry;
        public string Command => "/getcommands";

        public GetCommandsCommand(ITelegramResponder telegramResponder, IPluginRegistry pluginRegistry)
        {
            _telegramResponder = telegramResponder;
            _pluginRegistry = pluginRegistry;
        }

        public async Task ExecuteAsync(Message message)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("📦 _Available Plugin Commands:_");

            Console.WriteLine($"Plugin count in registry: {_pluginRegistry.Plugins.Count}");

            if (_pluginRegistry.Plugins.Count > 0)
            {
                foreach (Plugin plugin in _pluginRegistry.Plugins)
                {
                    foreach (PluginExecution pluginExecution in plugin.PluginExecutions)
                    {
                        sb.AppendLine($"🔹 *{pluginExecution.CommandName}* \\- `{pluginExecution.Help}`");
                    }
                }
            }
            else
            {
                sb.AppendLine("\t `(empty - add plugins)`");
            }

            var output = sb.ToString();
            await _telegramResponder.SendMarkdown(message.Chat, sb.ToString());
        }
    }
}
