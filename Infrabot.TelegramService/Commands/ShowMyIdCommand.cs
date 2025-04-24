using Infrabot.TelegramService.Core;
using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Commands
{
    public class ShowMyIdCommand : ICommandHandler
    {
        private readonly ITelegramResponder _telegramResponder;
        public string Command => "/showmyid";

        public ShowMyIdCommand(ITelegramResponder telegramResponder) { 
            _telegramResponder = telegramResponder;
        }

        public async Task ExecuteAsync(Message message)
        {
            await _telegramResponder.SendMarkdown(message.Chat, $"Your id is: *{message.From.Id.ToString().Replace("-", "\\-")}* and your username is *{message.From.Username}*\nChat id is: *{message.Chat.Id.ToString().Replace("-", "\\-")}*");
        }
    }
}
