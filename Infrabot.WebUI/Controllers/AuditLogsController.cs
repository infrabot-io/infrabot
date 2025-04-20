using Infrabot.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class AuditLogsController : Controller
    {
        private readonly ILogger<AuditLogsController> _logger;
        private readonly IAuditLogsService _auditLogsService;

        public AuditLogsController(
            ILogger<AuditLogsController> logger,
            IAuditLogsService auditLogsService)
        {
            _logger = logger;
            _auditLogsService = auditLogsService;
        }

        public async Task<IActionResult> Index()
        {
            var telegramMessages = await _auditLogsService.GetAuditLogs(0, 200);
            return View(telegramMessages);
        }
    }
}
