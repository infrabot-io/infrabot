using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Infrabot.Common.Contexts;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrabot.WebUI.Controllers
{
    public class PermissionAssignmentController : Controller
    {
        private readonly InfrabotContext _context;
        private readonly ILogger<PermissionAssignmentController> _logger;

        public PermissionAssignmentController(ILogger<PermissionAssignmentController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 0)
        {
            const int PageSize = 50;

            var count = _context.PermissionAssignments.Count() - 1;
            var assignments = await _context.PermissionAssignments.OrderBy(s => s.Name).Include(pa => pa.Plugins).Include(pa => pa.TelegramUsers).Include(pa => pa.Groups).Skip(page * PageSize).Take(PageSize).AsSplitQuery().ToListAsync();
            var maxpage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(assignments);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = new PermissionAssignmentViewModel
            {
                AvailablePlugins = await _context.Plugins.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync(),
                AvailableTelegramUsers = await _context.TelegramUsers.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} {u.Surname}"
                }).ToListAsync(),
                AvailableGroups = await _context.Groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToListAsync()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PermissionAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate at least one plugin is selected.
                if (!model.SelectedPluginIds.Any())
                {
                    ModelState.AddModelError(string.Empty, "At least one plugin must be selected.");
                }
                // Validate that at least one subject (user or group) is selected.
                if (!model.SelectedTelegramUserIds.Any() && !model.SelectedGroupIds.Any())
                {
                    ModelState.AddModelError(string.Empty, "At least one Telegram user or group must be selected.");
                }

                if (!ModelState.IsValid)
                {
                    PopulatePermissionAssignmentViewModel(model);
                    return View(model);
                }

                var permissionAssignment = new PermissionAssignment
                {
                    Name = model.Name // Save the assignment name.
                };

                // Associate selected plugins.
                var selectedPlugins = await _context.Plugins.Where(p => model.SelectedPluginIds.Contains(p.Id)).ToListAsync();
                foreach (var plugin in selectedPlugins)
                {
                    permissionAssignment.Plugins.Add(plugin);
                }

                // Associate selected Telegram users.
                var selectedUsers = await _context.TelegramUsers.Where(u => model.SelectedTelegramUserIds.Contains(u.Id)).ToListAsync();
                foreach (var user in selectedUsers)
                {
                    permissionAssignment.TelegramUsers.Add(user);
                }

                // Associate selected Groups.
                var selectedGroups = await _context.Groups.Where(g => model.SelectedGroupIds.Contains(g.Id)).ToListAsync();
                foreach (var group in selectedGroups)
                {
                    permissionAssignment.Groups.Add(group);
                }

                await _context.PermissionAssignments.AddAsync(permissionAssignment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            PopulatePermissionAssignmentViewModel(model);

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var permissionAssignment = await _context.PermissionAssignments.Include(pa => pa.Plugins).Include(pa => pa.TelegramUsers).Include(pa => pa.Groups).FirstOrDefaultAsync(pa => pa.Id == id);
            if (permissionAssignment == null)
            {
                return NotFound();
            }

            var model = new PermissionAssignmentViewModel
            {
                Id = permissionAssignment.Id,
                Name = permissionAssignment.Name, // Set the name
                SelectedPluginIds = permissionAssignment.Plugins.Select(p => p.Id).ToList(),
                SelectedTelegramUserIds = permissionAssignment.TelegramUsers.Select(u => u.Id).ToList(),
                SelectedGroupIds = permissionAssignment.Groups.Select(g => g.Id).ToList(),
                AvailablePlugins = await _context.Plugins.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync(),
                AvailableTelegramUsers = await _context.TelegramUsers.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} {u.Surname}"
                }).ToListAsync(),
                AvailableGroups = await _context.Groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToListAsync()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PermissionAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var permissionAssignment = await _context.PermissionAssignments.Include(pa => pa.Plugins).Include(pa => pa.TelegramUsers).Include(pa => pa.Groups).FirstOrDefaultAsync(pa => pa.Id == model.Id);
                if (permissionAssignment == null)
                {
                    return NotFound();
                }

                permissionAssignment.Name = model.Name;

                // Update Plugins.
                var pluginsToRemove = permissionAssignment.Plugins
                    .Where(p => !model.SelectedPluginIds.Contains(p.Id))
                    .ToList();
                foreach (var p in pluginsToRemove)
                {
                    permissionAssignment.Plugins.Remove(p);
                }
                var existingPluginIds = permissionAssignment.Plugins.Select(p => p.Id).ToList();
                var pluginsToAdd = await _context.Plugins.Where(p => model.SelectedPluginIds.Contains(p.Id) && !existingPluginIds.Contains(p.Id)).ToListAsync();
                foreach (var p in pluginsToAdd)
                {
                    permissionAssignment.Plugins.Add(p);
                }

                // Update TelegramUsers.
                var usersToRemove = permissionAssignment.TelegramUsers
                    .Where(u => !model.SelectedTelegramUserIds.Contains(u.Id))
                    .ToList();
                foreach (var u in usersToRemove)
                {
                    permissionAssignment.TelegramUsers.Remove(u);
                }
                var existingUserIds = permissionAssignment.TelegramUsers.Select(u => u.Id).ToList();
                var usersToAdd = await _context.TelegramUsers.Where(u => model.SelectedTelegramUserIds.Contains(u.Id) && !existingUserIds.Contains(u.Id)).ToListAsync();
                foreach (var u in usersToAdd)
                {
                    permissionAssignment.TelegramUsers.Add(u);
                }

                // Update Groups.
                var groupsToRemove = permissionAssignment.Groups
                    .Where(g => !model.SelectedGroupIds.Contains(g.Id))
                    .ToList();
                foreach (var g in groupsToRemove)
                {
                    permissionAssignment.Groups.Remove(g);
                }
                var existingGroupIds = permissionAssignment.Groups.Select(g => g.Id).ToList();
                var groupsToAdd = await _context.Groups.Where(g => model.SelectedGroupIds.Contains(g.Id) && !existingGroupIds.Contains(g.Id)).ToListAsync();
                foreach (var g in groupsToAdd)
                {
                    permissionAssignment.Groups.Add(g);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            PopulatePermissionAssignmentViewModel(model);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            var permissionAssignment = await _context.PermissionAssignments.FirstOrDefaultAsync(s => s.Id == Id);
            if (permissionAssignment is not null)
                return View(permissionAssignment);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int Id)
        {
            var permissionAssignment = await _context.PermissionAssignments.FindAsync(Id);

            if (permissionAssignment != null)
            {
                _context.PermissionAssignments.Remove(permissionAssignment);
                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.PermissionAssignment, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} deleted permission '{permissionAssignment.Name}'" });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Helper: Re-populate available lists if model state is invalid.
        private void PopulatePermissionAssignmentViewModel(PermissionAssignmentViewModel model)
        {
            model.AvailablePlugins = _context.Plugins.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
            model.AvailableTelegramUsers = _context.TelegramUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Name} {u.Surname}"
            }).ToList();
            model.AvailableGroups = _context.Groups.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();
        }
    }
}
