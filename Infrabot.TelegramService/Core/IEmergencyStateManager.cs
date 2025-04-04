namespace Infrabot.TelegramService.Core
{
    public interface IEmergencyStateManager
    {
        bool IsExitPrevented();
        void SetPreventionFlag();
        void ClearPreventionFlag();
    }
}
