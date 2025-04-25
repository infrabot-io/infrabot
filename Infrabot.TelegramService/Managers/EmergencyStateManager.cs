using Infrabot.TelegramService.Core;

namespace Infrabot.TelegramService.Managers
{
    public class EmergencyStateManager : IEmergencyStateManager
    {
        private readonly string lockFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emergency.lock");

        public bool IsExitPrevented() => File.Exists(lockFile);

        public void SetPreventionFlag()
        {
            if (!File.Exists(lockFile))
                File.Create(lockFile).Dispose();
        }

        public void ClearPreventionFlag()
        {
            if (File.Exists(lockFile))
                File.Delete(lockFile);
        }
    }
}
