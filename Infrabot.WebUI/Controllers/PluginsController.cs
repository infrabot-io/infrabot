using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Infrabot.Common.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infrabot.PluginSystem.Utils;

namespace Infrabot.WebUI.Controllers
{
    public class PluginsController : Controller
    {
        private static readonly string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        private readonly InfrabotContext _context;
        private readonly ILogger<PluginsController> _logger;

        public PluginsController(ILogger<PluginsController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 0)
        {
            const int PageSize = 50;

            var count = _context.Plugins.Count() - 1;
            var plugins = await _context.Plugins.OrderBy(s => s.Name).Skip(page * PageSize).Take(PageSize).ToListAsync();
            var maxpage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(plugins);
        }

        [Authorize]
        public async Task<IActionResult> View(int Id)
        {
            var plugin = await _context.Plugins.FirstOrDefaultAsync(s => s.Id == Id);

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
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
                return RedirectToAction("Index");

            return View(plugin);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            var plugin = await _context.Plugins.FirstOrDefaultAsync(s => s.Id == Id);
            if (plugin is not null)
                return View(plugin);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int Id)
        {
            var plugin = await _context.Plugins.FindAsync(Id);

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
                    Console.WriteLine(ex.ToString());
                }

                _context.Plugins.Remove(plugin);
                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.Plugin, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} deleted plugin '{plugin.Name}'" });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
