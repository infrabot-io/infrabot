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
        private readonly IAuditLogService _auditLogService;

        public ConfigurationController(
            ILogger<ConfigurationController> logger,
            IConfigurationService configurationService,
            IAuditLogService auditLogService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index()
        {
            var configuration = await _configurationService.GetConfiguration();

            if (configuration is null)
            {
                await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Access, LogItem = AuditLogItem.Configuration, LogResult = AuditLogResult.Error, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User} accessed Configuration page but got error about system configuration absense" });
                _logger.LogError("System is corrupted. No configuration in the database has been found.");
                return NotFound();
            }

            ViewBag.ConfigurationSaved = TempData[TempDataKeys.ConfigurationSaved];

            return View(configuration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Configuration configuration)
        {
            if (ModelState.IsValid)
            {
                await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.Configuration, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User} changed system configuration" });
                await _configurationService.UpdateConfiguration(configuration);

                _logger.LogInformation("Configuration saved: " + JsonConvert.SerializeObject(configuration));

                TempData[TempDataKeys.ConfigurationSaved] = true;

                return RedirectToAction("Index");
            }

            return View(configuration);
        }
    }
}
