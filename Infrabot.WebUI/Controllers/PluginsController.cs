using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.PluginSystem.Utils;
using Infrabot.WebUI.Services;
using Infrabot.WebUI.Utils;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Models;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class PluginsController : Controller
    {
        private readonly ILogger<PluginsController> _logger;
        private readonly IPluginsService _pluginsService;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;
        private string _pluginDirectory;

        public PluginsController(
            ILogger<PluginsController> logger,
            IPluginsService pluginsService, 
            IAuditLogService auditLogService,
            IConfiguration configuration)
        {
            _logger = logger;
            _pluginsService = pluginsService;
            _auditLogService = auditLogService;
            _configuration = configuration;

            _pluginDirectory = PathNormalizer.NormalizePath(_configuration["Plugins:PluginsDirectory"] ?? "plugins");
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;

            var count = await _pluginsService.GetPluginsCount() - 1;
            var plugins = await _pluginsService.GetPlugins(page, pageSize);
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            ViewBag.PluginNotFound = TempData[TempDataKeys.PluginNotFound] as bool?;
            ViewBag.PluginDeleted = TempData[TempDataKeys.PluginDeleted] as bool?;

            return View(plugins);
        }

        public async Task<IActionResult> View(int id)
        {
            var plugin = await _pluginsService.GetPluginById(id);

            if (plugin is not null)
            {
                try
                {
                    string[] files = Directory.GetFiles(_pluginDirectory, "*.plug");

                    foreach (string file in files)
                    {
                        var pluginFile = await PluginUtility.GetPlugin(file);

                        if (pluginFile.Guid != plugin.Guid)
                            continue;

                        ViewBag.Id = pluginFile.Id;
                        ViewBag.Description = pluginFile.Description;
                        ViewBag.Author = pluginFile.Author;
                        ViewBag.Version = pluginFile.Version;
                        ViewBag.WebSite = pluginFile.WebSite;
                        ViewBag.PluginExecutions = pluginFile.PluginExecutions;

                        return View(plugin);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to read Plugin file: " + ex.ToString());
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var plugin = await _pluginsService.GetPluginById(id);

            if (plugin is null)
            {
                TempData[TempDataKeys.PluginNotFound] = true;
                return RedirectToAction("Index");
            }

            return View(plugin);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int id)
        {
            if (ModelState.IsValid)
            {
                var plugin = await _pluginsService.GetPluginById(id);

                if (plugin is null)
                {
                    TempData[TempDataKeys.PluginNotFound] = true;
                    return RedirectToAction("Index");
                }

                try
                {
                    string[] files = Directory.GetFiles(_pluginDirectory, "*.plug");

                    foreach (string file in files)
                    {
                        var pluginFile = await PluginUtility.GetPlugin(file);

                        if (pluginFile.Guid != plugin.Guid)
                            continue;

                        System.IO.File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to delete Plugin file: " + ex.ToString());
                }

                await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.Plugin, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} deleted plugin {plugin.Name} with guid {plugin.Guid}" });
                await _pluginsService.DeletePlugin(plugin);
                TempData[TempDataKeys.PluginDeleted] = true;
            }

            return RedirectToAction("Index");
        }

        public IActionResult Upload()
        {
            var model = new UploadPluginViewModel
            {
                Files = null
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadPluginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if(model.Files is not null)
                {
                    foreach (var file in model.Files)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                            string filePath = Path.Combine(_pluginDirectory, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                var pluginFile = PluginUtility.GetPlugin(file.OpenReadStream());
                                var plugin = await _pluginsService.GetPluginByGuid(pluginFile.Guid);

                                if (plugin is not null)
                                {
                                    _logger.LogWarning($"Will not upload file {file.FileName} because plugin with the same GUID {pluginFile.Guid} already is present.");
                                    continue;
                                }

                                await file.CopyToAsync(stream);
                            }
                        }
                    }
                }
            }

            return View(model);
        }
    }
}
