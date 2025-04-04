using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using infrabot.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Infrabot.Common.Contexts;
using Newtonsoft.Json;
using Infrabot.WebUI.Constants;

namespace Infrabot.WebUI.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly InfrabotContext _context;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(ILogger<ConfigurationController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var configuration = await _context.Configurations.FirstOrDefaultAsync(s => s.Id == 1);

            if (configuration is null)
            {
                _logger.LogError("System is corrupted. No configuration in the database has been found.");
                return NotFound();
            }

            ViewBag.ConfigurationSaved = TempData[TempDataKeys.ConfigurationSaved];

            return View(configuration);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Configuration configuration)
        {
            if (ModelState.IsValid)
            {
                _context.Configurations.Update(configuration);
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Update, LogItem = AuditLogItem.Configuration, CreatedDate = DateTime.Now, Description = $"Configuration has been changed by {HttpContext.User.FindFirstValue("Login")}" });
                await _context.SaveChangesAsync();

                _logger.LogInformation("Configuration saved: " + JsonConvert.SerializeObject(configuration));

                TempData[TempDataKeys.ConfigurationSaved] = true;

                return RedirectToAction("Index");
            }

            return View(configuration);
        }
    }
}
