using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IAuditLogsService _auditLogsService;

        public ConfigurationController(
            ILogger<ConfigurationController> logger,
            IConfigurationService configurationService,
            IAuditLogsService auditLogsService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _auditLogsService = auditLogsService;
        }

        public async Task<IActionResult> Index()
        {
            var configuration = await _configurationService.GetConfiguration();

            if (configuration is null)
            {
                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Access, LogItem = AuditLogItem.Configuration, LogResult = AuditLogResult.Error, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} accessed Configuration page but got error about system configuration absense" });
                _logger.LogError("System is corrupted. No configuration in the database has been found. Please delete database and restart application.");
                return NotFound();
            }

            return View(configuration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Configuration configuration)
        {
            if (ModelState.IsValid)
            {
                string serializedConfiguration = JsonConvert.SerializeObject(configuration);
                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.Configuration, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} changed system configuration to: {serializedConfiguration}" });
                await _configurationService.UpdateConfiguration(configuration);

                _logger.LogInformation("Configuration saved: " + serializedConfiguration);

                ViewData[TempDataKeys.ConfigurationSaved] = true;
            }

            return View(configuration);
        }
    }
}
