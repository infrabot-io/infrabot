using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.PluginSystem.Utils;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class PluginsController : Controller
    {
        private readonly ILogger<PluginsController> _logger;
        private readonly IPluginsService _pluginsService;
        private readonly IAuditLogService _auditLogService;
        private static readonly string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        public PluginsController(ILogger<PluginsController> logger, IPluginsService pluginsService, IAuditLogService auditLogService)
        {
            _logger = logger;
            _pluginsService = pluginsService;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;

            var count = await _pluginsService.GetPluginsCount();
            var plugins = await _pluginsService.GetPlugins(page, pageSize);
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(plugins);
        }

        public async Task<IActionResult> View(int id)
        {
            var plugin = await _pluginsService.GetPluginById(id);

            if (plugin is not null)
            {
                try
                {
                    string[] files = Directory.GetFiles(pluginsPath, "*.plug");

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

            if (plugin is not null)
                return View(plugin);
            
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int id)
        {
            if (ModelState.IsValid)
            {
                var plugin = await _pluginsService.GetPluginById(id);

                if (plugin is not null)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(pluginsPath, "*.plug");

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

                    await _pluginsService.DeletePlugin(plugin);
                    await _auditLogService.AddAuditLog(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.Plugin, CreatedDate = DateTime.Now, Description = $"User {this.User} deleted plugin '{plugin.Name}'" });
                }
            }

            return RedirectToAction("Index");
        }
    }
}
