using infrabot.Controllers;
using Infrabot.Common.Contexts;
using Infrabot.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly InfrabotContext _context;

        public ApiController(ILogger<ApiController> logger, InfrabotContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("getresourcemetrics")]
        public async Task<string> GetResourceMetrics()
        {
            var metrics = await _context.HealthChecks.OrderByDescending(x => x.CreatedDate).Take(7).ToListAsync();
            return JsonConvert.SerializeObject(metrics);
        }

        [HttpGet("getstats")]
        public async Task<string> GetStats()
        {
            StatsItem statsItem = new StatsItem();

            statsItem.Plugins = await _context.Plugins.CountAsync();
            statsItem.TelegramUsers = await _context.TelegramUsers.CountAsync();
            statsItem.Users = await _context.Users.CountAsync();

            var events = await _context.EventLogs.OrderByDescending(x => x.CreatedDate).Take(15).ToListAsync();
            foreach (var _event in events)
            {
                statsItem.StatsEvents.Add(
                    new StatsEvent
                    {
                        Description = _event.Description,
                        EventDate = _event.CreatedDate,
                        EventType = _event.EventType
                    }
                );
            }

            return JsonConvert.SerializeObject(statsItem);
        }

        [HttpGet("getmessagestats")]
        public async Task<string> GetMessageStats()
        {
            var now = DateTime.Now;
            var startOfDay = DateTime.UtcNow.Date;

            var messageCounts = await _context.TelegramMessages
                .Where(m => m.CreatedDate >= startOfDay) // Ensure this is in the correct time zone
                .GroupBy(m => (m.CreatedDate.Hour / 4) * 4) // Groups messages into 4-hour intervals
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Ensure all time slots exist (fill missing hours with 0 messages)
            var allHours = new List<int> { 0, 4, 8, 12, 16, 20 };
            var messageData = allHours.Select(hour => new
            {
                Hour = hour,
                Count = messageCounts.FirstOrDefault(m => m.Hour == hour)?.Count ?? 0
            });

            return JsonConvert.SerializeObject(messageData);
        }

        [HttpGet("getpluginstats")]
        public async Task<string> GetPluginStats()
        {
            var pluginCounts = await _context.Plugins
                .GroupBy(p => p.Name)
                .Select(g => new
                {
                    PluginType = g.Key.ToString(), // Convert enum to string
                    Count = g.Count()
                })
                .ToListAsync();

            return JsonConvert.SerializeObject(pluginCounts);
        }
    }
}
