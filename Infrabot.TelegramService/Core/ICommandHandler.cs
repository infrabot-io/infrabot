using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Core
{
    public interface ICommandHandler
    {
        string Command { get; }
        Task ExecuteAsync(Message message);
    }
}
