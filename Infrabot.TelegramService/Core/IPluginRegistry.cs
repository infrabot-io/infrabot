using Infrabot.PluginSystem;

namespace Infrabot.TelegramService.Core
{
    public interface IPluginRegistry
    {
        IReadOnlyList<Plugin> Plugins { get; }
        public void RefreshPlugins();
    }
}
