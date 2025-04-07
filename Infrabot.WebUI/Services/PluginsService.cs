using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IPluginsService
    {
        Task<IEnumerable<Plugin>> GetPlugins(int page = 0, int pageSize = 50);
        Task<Plugin> GetPluginById(int id);
        Task<IEnumerable<Plugin>> GetAllPlugins();
        Task<int> GetPluginsCount();
        Task DeletePlugin(Plugin plugin);
    }

    public class PluginsService : IPluginsService
    {
        private readonly InfrabotContext _context;

        public PluginsService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Plugin>> GetPlugins(int page = 0, int pageSize = 50)
        {
            var plugins = await _context.Plugins.OrderBy(s => s.Name).Skip(page * pageSize).Take(pageSize).ToListAsync();
            return plugins;
        }

        public async Task<IEnumerable<Plugin>> GetAllPlugins()
        {
            var plugins = await _context.Plugins.ToListAsync();
            return plugins;
        }
        public async Task<Plugin> GetPluginById(int id)
        {
            var plugin = await _context.Plugins.FirstOrDefaultAsync(s => s.Id == id);
            return plugin;
        }

        public async Task<int> GetPluginsCount()
        {
            int pluginsCount = await _context.Plugins.CountAsync();
            return pluginsCount;
        }
        public async Task DeletePlugin(Plugin plugin)
        {
            _context.Plugins.Remove(plugin);
            await _context.SaveChangesAsync();
        }
    }
}
