using Infrabot.TelegramService.Core;
using Telegram.Bot.Types;

namespace Infrabot.TelegramService.Commands
{
    public class EmergencyCommand : ICommandHandler
    {
        private readonly ITelegramResponder _telegramResponder;
        private readonly IEmergencyStateManager _stateManager;
        public string Command => "/emergency";

        public EmergencyCommand(ITelegramResponder telegramResponder, IEmergencyStateManager stateManager)
        {
            _telegramResponder = telegramResponder;
            _stateManager = stateManager;
        }

        public async Task ExecuteAsync(Message message)
        {
            // If we find specified file then delete it, and prevent application exit. 
            // In any other case application goes to exit loop and can not stop receiving /emergency command
            // Who knows why this stupid ass shit keeps happening
            if (_stateManager.IsExitPrevented())
            {
                _stateManager.ClearPreventionFlag();
                return;
            }

            await _telegramResponder.SendMarkdown(message.Chat, $"*Shutting down program immediately*");

            _stateManager.SetPreventionFlag();
            Environment.Exit(0);
        }
    }
}
