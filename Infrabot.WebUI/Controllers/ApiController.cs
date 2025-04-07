using Infrabot.Common.Domain;
using Infrabot.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly IApiService _apiService;
        private readonly IUsersService _userService;

        public ApiController(ILogger<ApiController> logger, IApiService apiService, IUsersService userService)
        {
            _logger = logger;
            _apiService = apiService;
            _userService = userService;
        }

        [HttpGet("getresourcemetrics")]
        public async Task<ActionResult> GetResourceMetrics()
        {
            var metrics = await _apiService.GetResourceMetrics();
            return Ok(metrics);
        }

        [HttpGet("getstats")]
        public async Task<ActionResult> GetStats()
        {
            StatsItem statsItem = new StatsItem();

            statsItem.Plugins = await _apiService.GetPluginsCount();
            statsItem.TelegramUsers = await _apiService.GetTelegramUsersCount();
            statsItem.Users = await _userService.GetUsersCount();

            var events = await _apiService.GetStats();
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

            return Ok(statsItem);
        }

        [HttpGet("getmessagestats")]
        public async Task<ActionResult> GetMessageStats()
        {
            // Get message counts
            var messageCounts = await _apiService.GetMessageStats();

            // Ensure all time slots exist (fill missing hours with 0 messages)
            var allHours = new List<int> { 0, 4, 8, 12, 16, 20 };
            var messageData = allHours.Select(hour => new
            {
                Hour = hour,
                Count = messageCounts.FirstOrDefault(m => m.Hour == hour)?.Count ?? 0
            });

            return Ok(messageData);
        }

        [HttpGet("getpluginstats")]
        public async Task<ActionResult> GetPluginStats()
        {
            var pluginCounts = await _apiService.GetPluginStats();
            return Ok(pluginCounts);
        }
    }
}
