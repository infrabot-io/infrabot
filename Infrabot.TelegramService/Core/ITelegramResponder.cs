using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Core
{
    public interface ITelegramResponder
    {
        Task SendPlain(Chat chat, string message, int maxLength = 12000);
        Task SendMarkdown(Chat chat, string message, int maxLength = 12000);
    }
}
