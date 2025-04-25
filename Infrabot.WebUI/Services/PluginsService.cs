using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Infrabot.WebUI.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IPluginsService
    {
        Task<IEnumerable<Plugin>> GetPlugins(int page = 0, int pageSize = 50);
        Task<Plugin> GetPluginById(int id);
        Task<Plugin> GetPluginByGuid(Guid guid);
        Task<IEnumerable<Plugin>> GetAllPlugins();
        Task<int> GetPluginsCount();
        Task DeletePlugin(Plugin plugin);
        Task<IEnumerable<Plugin>> AssociateSelectedPluginsForPermission(PermissionAssignmentViewModel model);
        Task<IEnumerable<Plugin>> RepopulatePluginsForPermissionUpdate(PermissionAssignmentViewModel model, IEnumerable<int> existingPluginIds);
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

        public async Task<Plugin> GetPluginByGuid(Guid guid)
        {
            var plugin = await _context.Plugins.FirstOrDefaultAsync(s => s.Guid == guid);
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

        public async Task<IEnumerable<Plugin>> AssociateSelectedPluginsForPermission(PermissionAssignmentViewModel model)
        {
            var plugins = await _context.Plugins.Where(p => model.SelectedPluginIds.Contains(p.Id)).ToListAsync();
            return plugins;
        }

        public async Task<IEnumerable<Plugin>> RepopulatePluginsForPermissionUpdate(PermissionAssignmentViewModel model, IEnumerable<int> existingPluginIds)
        {
            var plugins = await _context.Plugins.Where(p => model.SelectedPluginIds.Contains(p.Id) && !existingPluginIds.Contains(p.Id)).ToListAsync();
            return plugins;
        }
    }
}
