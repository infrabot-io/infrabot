﻿using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Infrabot.WebUI.Services;
using Infrabot.WebUI.Constants;

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
        private readonly IAuditLogsService _auditLogsService;

        public PermissionAssignmentController(
            ILogger<PermissionAssignmentController> logger,
            IPermissionAssignmentService permissionAssignmentService,
            IPluginsService pluginsService,
            ITelegramUsersService telegramUsersService,
            IGroupsService groupsService,
            IAuditLogsService auditLogsService)
        {
            _logger = logger;
            _permissionAssignmentService = permissionAssignmentService;
            _pluginsService = pluginsService;
            _telegramUsersService = telegramUsersService;
            _groupsService = groupsService;
            _auditLogsService = auditLogsService;
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

            ViewBag.PermissionAssignmentNotFound = TempData[TempDataKeys.PermissionAssignmentNotFound] as bool?;
            ViewBag.PermissionAssignmentDeleted = TempData[TempDataKeys.PermissionAssignmentDeleted] as bool?;

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
                string pluginsList = "";
                string usersList = "";
                string groupsList = "";

                // Validate at least one plugin is selected.
                if (!model.SelectedPluginIds.Any())
                {
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.PermissionAssignment, LogResult = AuditLogResult.Denied, LogSeverity = AuditLogSeverity.Low, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} tried to create permission assignment but forgot to select any plugin" });
                    ViewData[TempDataKeys.PermissionAssignmentOnePluginMustBeSelected] = true;
                    await PopulatePermissionAssignmentViewModel(model); 
                    return View(model);
                }

                // Validate that at least one subject (user or group) is selected.
                if (!model.SelectedTelegramUserIds.Any() && !model.SelectedGroupIds.Any())
                {
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.PermissionAssignment, LogResult = AuditLogResult.Denied, LogSeverity = AuditLogSeverity.Low, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} tried to create permission assignment but forgot to select any user or group" });
                    ViewData[TempDataKeys.PermissionAssignmentOneTelegramUserMustBeSelected] = true;
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
                    pluginsList += $"{plugin.Name} ,";
                }

                // Associate selected Telegram users.
                var selectedUsers = await _telegramUsersService.AssociateSelectedTelegramUsersForPermission(model);
                foreach (var user in selectedUsers)
                {
                    permissionAssignment.TelegramUsers.Add(user);
                    usersList += $"{user.Name} {user.TelegramId},";
                }

                // Associate selected Groups.
                var selectedGroups = await _groupsService.AssociateSelectedGroupsForPermission(model);
                foreach (var group in selectedGroups)
                {
                    permissionAssignment.Groups.Add(group);
                    groupsList += $"{group.Name},";
                }

                // Create new permission assignment
                await _permissionAssignmentService.CreatePermissionAssignment(permissionAssignment);
                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.PermissionAssignment, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} created permission assignment {permissionAssignment.Name} with plugins '{pluginsList}' for users '{usersList}' and groups '{groupsList}'" });

                return RedirectToAction("Index");
            }

            await PopulatePermissionAssignmentViewModel(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(id);

            if (permissionAssignment is null)
            {
                TempData[TempDataKeys.PermissionAssignmentNotFound] = true;
                return RedirectToAction("Index");
            }

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
                string pluginsList = "";
                string usersList = "";
                string groupsList = "";

                var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(model.Id);

                if (permissionAssignment is null)
                {
                    return RedirectToAction("Index");
                }

                permissionAssignment.Name = model.Name;

                // Update Plugins.
                var pluginsToRemove = permissionAssignment.Plugins.Where(p => !model.SelectedPluginIds.Contains(p.Id)).ToList();
                foreach (var p in pluginsToRemove)
                {
                    TempData[TempDataKeys.PermissionAssignmentNotFound] = true;
                    permissionAssignment.Plugins.Remove(p);
                }

                var existingPluginIds = permissionAssignment.Plugins.Select(p => p.Id).ToList();
                
                var pluginsToAdd = await _pluginsService.RepopulatePluginsForPermissionUpdate(model, existingPluginIds);
                foreach (var p in pluginsToAdd)
                {
                    permissionAssignment.Plugins.Add(p);
                    pluginsList += $"{p.Name} ,";
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
                    usersList += $"{u.Name} {u.TelegramId},";
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
                    groupsList += $"{g.Name},";
                }

                await _permissionAssignmentService.UpdatePermissionAssignment(permissionAssignment);
                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.PermissionAssignment, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} updated permission assignment {permissionAssignment.Name} with plugins '{pluginsList}' for users '{usersList}' and groups '{groupsList}'" });

                ViewData[TempDataKeys.PermissionAssignmentSaved] = true;
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(id);

            if (permissionAssignment is null)
            {
                TempData[TempDataKeys.PermissionAssignmentNotFound] = true;
                return RedirectToAction("Index");
            }

            return View(permissionAssignment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int id)
        {
            if (ModelState.IsValid)
            {
                var permissionAssignment = await _permissionAssignmentService.GetPermissionAssignmentById(id);

                if (permissionAssignment is null)
                {
                    TempData[TempDataKeys.PermissionAssignmentNotFound] = true;
                    return RedirectToAction("Index");
                }

                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.PermissionAssignment, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} deleted permission assignment {permissionAssignment.Name}" });
                await _permissionAssignmentService.DeletePermissionAssignment(permissionAssignment);
                TempData[TempDataKeys.PermissionAssignmentDeleted] = true;
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
