using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class PermissionAssignmentController : Controller
    {
        private readonly ILogger<PermissionAssignmentController> _logger;
        private readonly IPermissionAssignmentService _permissionAssignmentService;
        private readonly IPluginsService _pluginsService;
        private readonly ITelegramUsersService _telegramUsersService;
        private readonly IGroupsService _groupsService;
        private readonly IAuditLogService _auditLogService;

        public PermissionAssignmentController(
            ILogger<PermissionAssignmentController> logger,
            IPermissionAssignmentService permissionAssignmentService,
            IPluginsService pluginsService,
            ITelegramUsersService telegramUsersService,
            IGroupsService groupsService,
            IAuditLogService auditLogService)
        {
            _logger = logger;
            _permissionAssignmentService = permissionAssignmentService;
            _pluginsService = pluginsService;
            _telegramUsersService = telegramUsersService;
            _groupsService = groupsService;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;
            var count = await _permissionAssignmentService.GetPermissionAssignmentsCount() - 1;
            var assignments = await _permissionAssignmentService.GetPermissionAssignments();
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(assignments);
        }

        public async Task<IActionResult> Create()
        {
            var plugins = await _pluginsService.GetAllPlugins();
            var telegramUsers = await _telegramUsersService.GetAllTelegramUsers();
            var groups = await _groupsService.GetAllGroups();

            var model = new PermissionAssignmentViewModel
            {
                AvailablePlugins = plugins.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList(),
                AvailableTelegramUsers = telegramUsers.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} {u.Surname}"
                }).ToList(),
                AvailableGroups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToList()
            };

            return View(model);
        }

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

                // Model must be valid
                if (!ModelState.IsValid)
                {
                    await PopulatePermissionAssignmentViewModel(model);
                    return View(model);
                }

                // Create PermissionAssignment object
                var permissionAssignment = new PermissionAssignment
                {
                    Name = model.Name
                };

                // Associate selected plugins.
                var selectedPlugins = await _pluginsService.AssociateSelectedPluginsForPermission(model);
                foreach (var plugin in selectedPlugins)
                {
                    permissionAssignment.Plugins.Add(plugin);
                }

                // Associate selected Telegram users.
                var selectedUsers = await _telegramUsersService.AssociateSelectedTelegramUsersForPermission(model);
                foreach (var user in selectedUsers)
                {
                    permissionAssignment.TelegramUsers.Add(user);
                }

                // Associate selected Groups.
                var selectedGroups = await _groupsService.AssociateSelectedGroupsForPermission(model);
                foreach (var group in selectedGroups)
                {
                    permissionAssignment.Groups.Add(group);
                }

                // Create new permission assignment
                await _permissionAssignmentService.CreatePermissionAssignment(permissionAssignment);

                return RedirectToAction("Index");
            }

            await PopulatePermissionAssignmentViewModel(model);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(id);

            if (permissionAssignment is null)
                return RedirectToAction("Index");

            var plugins = await _pluginsService.GetAllPlugins();
            var telegramUsers = await _telegramUsersService.GetAllTelegramUsers();
            var groups = await _groupsService.GetAllGroups();

            var model = new PermissionAssignmentViewModel
            {
                Id = permissionAssignment.Id,
                Name = permissionAssignment.Name, // Set the name
                SelectedPluginIds = permissionAssignment.Plugins.Select(p => p.Id).ToList(),
                SelectedTelegramUserIds = permissionAssignment.TelegramUsers.Select(u => u.Id).ToList(),
                SelectedGroupIds = permissionAssignment.Groups.Select(g => g.Id).ToList(),

                AvailablePlugins = plugins.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList(),
                AvailableTelegramUsers = telegramUsers.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Name} {u.Surname}"
                }).ToList(),
                AvailableGroups = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PermissionAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(model.Id);

                if (permissionAssignment is null)
                    return RedirectToAction("Index");

                permissionAssignment.Name = model.Name;

                // Update Plugins.
                var pluginsToRemove = permissionAssignment.Plugins.Where(p => !model.SelectedPluginIds.Contains(p.Id)).ToList();
                foreach (var p in pluginsToRemove)
                {
                    permissionAssignment.Plugins.Remove(p);
                }

                var existingPluginIds = permissionAssignment.Plugins.Select(p => p.Id).ToList();
                
                var pluginsToAdd = await _pluginsService.RepopulatePluginsForPermissionUpdate(model, existingPluginIds);
                foreach (var p in pluginsToAdd)
                {
                    permissionAssignment.Plugins.Add(p);
                }

                // Update TelegramUsers.
                var usersToRemove = permissionAssignment.TelegramUsers.Where(u => !model.SelectedTelegramUserIds.Contains(u.Id)).ToList();
                foreach (var u in usersToRemove)
                {
                    permissionAssignment.TelegramUsers.Remove(u);
                }

                var existingUserIds = permissionAssignment.TelegramUsers.Select(u => u.Id).ToList();
                
                var usersToAdd = await _telegramUsersService.RepopulateTelegramUsersForPermissionUpdate(model, existingUserIds);
                foreach (var u in usersToAdd)
                {
                    permissionAssignment.TelegramUsers.Add(u);
                }

                // Update Groups.
                var groupsToRemove = permissionAssignment.Groups.Where(g => !model.SelectedGroupIds.Contains(g.Id)).ToList();

                foreach (var g in groupsToRemove)
                {
                    permissionAssignment.Groups.Remove(g);
                }
                
                var existingGroupIds = permissionAssignment.Groups.Select(g => g.Id).ToList();
                
                var groupsToAdd = await _groupsService.RepopulateGroupsForPermissionUpdate(model, existingGroupIds);
                foreach (var g in groupsToAdd)
                {
                    permissionAssignment.Groups.Add(g);
                }

                await _permissionAssignmentService.UpdatePermissionAssignment(permissionAssignment);

                return RedirectToAction("Index");
            }

            await PopulatePermissionAssignmentViewModel(model);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(id);

            if (permissionAssignment is not null)
                return View(permissionAssignment);
            else
                return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int id)
        {
            if (ModelState.IsValid)
            {
                var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(id);

                if (permissionAssignment != null)
                {
                    await _permissionAssignmentService.DeletePermissionAssignment(permissionAssignment);
                    await _auditLogService.AddAuditLog(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.PermissionAssignment, CreatedDate = DateTime.Now, Description = $"User {this.User} deleted deleted permission '{permissionAssignment.Name}'" });
                }
            }

            return RedirectToAction("Index");
        }

        // Helper: Re-populate available lists if model state is invalid.
        private async Task PopulatePermissionAssignmentViewModel(PermissionAssignmentViewModel model)
        {
            var plugins = await _pluginsService.GetAllPlugins();
            var telegramUsers = await _telegramUsersService.GetAllTelegramUsers();
            var groups = await _groupsService.GetAllGroups();

            model.AvailablePlugins = plugins.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
            model.AvailableTelegramUsers = telegramUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Name} {u.Surname}"
            }).ToList();
            model.AvailableGroups = groups.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();
        }
    }
}
