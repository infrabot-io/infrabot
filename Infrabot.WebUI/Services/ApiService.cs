using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IApiService
    {
        Task<IEnumerable<HealthCheck>> GetResourceMetrics(int limit = 7);
        Task<IEnumerable<EventLog>> GetStats(int limit = 15);
        Task<IEnumerable<MessageStat>> GetMessageStats();
        Task<IEnumerable<PluginStat>> GetPluginStats();
        Task<int> GetPluginsCount();
        Task<int> GetTelegramUsersCount();
        Task<int> GetUsersCount();
    }

    public class ApiService : IApiService
    {
        private readonly InfrabotContext _context;

        public ApiService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HealthCheck>> GetResourceMetrics(int limit = 7)
        {
            var metrics = await _context.HealthChecks.OrderByDescending(x => x.CreatedDate).Take(limit).ToListAsync();
            return metrics;
        }

        public async Task<IEnumerable<EventLog>> GetStats(int limit = 15)
        {
            var events = await _context.EventLogs.OrderByDescending(x => x.CreatedDate).Take(limit).ToListAsync();
            return events;
        }

        public async Task<IEnumerable<MessageStat>> GetMessageStats()
        {
            var now = DateTime.Now;
            var startOfDay = DateTime.Now.Date;

            var messageCounts = await _context.TelegramMessages
                .Where(m => m.CreatedDate >= startOfDay) // Ensure this is in the correct time zone
                .GroupBy(m => (m.CreatedDate.Hour / 4) * 4) // Groups messages into 4-hour intervals
                .Select(g => new MessageStat
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return messageCounts;
        }

        public async Task<IEnumerable<PluginStat>> GetPluginStats()
        {
            var pluginCounts = await _context.Plugins
                .GroupBy(p => p.Name)
                .Select(g => new PluginStat
                {
                    PluginType = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            return pluginCounts;
        }

        public async Task<int> GetPluginsCount()
        {
            int pluginCounts = await _context.Plugins.CountAsync();
            return pluginCounts;
        }

        public async Task<int> GetTelegramUsersCount()
        {
            int pluginCounts = await _context.TelegramUsers.CountAsync();
            return pluginCounts;
        }

        public async Task<int> GetUsersCount()
        {
            int pluginCounts = await _context.Users.CountAsync();
            return pluginCounts;
        }
    }
}
