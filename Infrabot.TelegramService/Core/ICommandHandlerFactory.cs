using Infrabot.Common.Models;
using Telegram.Bot;

namespace Infrabot.TelegramService.Core
{
    public interface ICommandHandlerFactory
    {
        List<ICommandHandler> GetBuiltInCommands();
        ITelegramBotClient GetBotClient();
        IPluginRegistry GetPluginRegistry();
        Configuration GetConfiguration();
        ITelegramResponder GetTelegramResponder();
        IBotCommandsUpdater GetBotCommandsUpdater();
        IServiceScopeFactory GetServiceScopeFactory();
    }
}
