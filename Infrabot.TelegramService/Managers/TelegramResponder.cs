using Infrabot.TelegramService.Core;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrabot.TelegramService.Managers
{
    public class TelegramResponder : ITelegramResponder
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramResponder> _logger;

        public TelegramResponder(ITelegramBotClient botClient, ILogger<TelegramResponder> logger)
        {
            _botClient = botClient;
            _logger = logger;
            _logger.LogInformation("Init: Telegram responder");
        }

        public async Task SendPlain(Chat chat, string message, int maxLength = 12000)
        {
            try
            {
                await SendSplit(chat, message, ParseMode.None, maxLength);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError("Error while sending message: " + ex.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while sending message: " + ex.ToString());
            }
        }

        public async Task SendMarkdown(Chat chat, string message, int maxLength = 12000)
        {
            try
            {
                await SendSplit(chat, message, ParseMode.MarkdownV2, maxLength);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError("Error while sending message: " + ex.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while sending message: " + ex.ToString());
            }
        }

        private async Task SendSplit(Chat chat, string text, ParseMode parseMode, int maxLength)
        {
            for (int i = 0; i < text.Length; i += 4096)
            {
                string chunk = text.Substring(i, Math.Min(4096, text.Length - i));
                await _botClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: chunk,
                    parseMode: parseMode
                );

                if (i > maxLength)
                {
                    _logger.LogWarning("Message exceeded max length ({Max}). Output was truncated.", maxLength);
                    break;
                }
            }
        }
    }
}
