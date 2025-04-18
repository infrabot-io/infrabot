using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ILogger<GroupsController> _logger;
        private readonly IAuditLogService _auditLogService;
        private readonly IGroupsService _groupsService;
        private readonly ITelegramUsersService _telegramUsersService;
        private readonly IUserGroupsService _userGroupsService;

        public GroupsController(
            ILogger<GroupsController> logger, 
            IAuditLogService auditLogService, 
            IGroupsService groupsService, 
            ITelegramUsersService telegramUsersService,
            IUserGroupsService userGroupsService)
        {
            _logger = logger;
            _auditLogService = auditLogService;
            _groupsService = groupsService;
            _telegramUsersService = telegramUsersService;
            _userGroupsService = userGroupsService;
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;

            var count = await _groupsService.GetGroupsCount() - 1;
            var groups = await _groupsService.GetGroups(page, pageSize);
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            ViewBag.GroupNotFound = TempData[TempDataKeys.GroupNotFound] as bool?;

            return View(groups);
        }

        public async Task<IActionResult> Create()
        {
            var telegramUsers = await _telegramUsersService.GetAllTelegramUsers();
            var groupViewModel = new GroupViewModel
            {
                AvailableTelegramUsers = telegramUsers.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name + " " + p.Surname }).ToList()
            };

            return View(groupViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel groupViewModel)
        {
            if (ModelState.IsValid)
            {
                var existingGroup = await _groupsService.GetGroupByName(groupViewModel.Name);

                if (existingGroup != null)
                {
                    ViewData[TempDataKeys.GroupAlreadyExists] = true;
                    return View(groupViewModel);
                }

                var group = new Group
                {
                    Name = groupViewModel.Name,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _groupsService.CreateGroup(group);

                if (groupViewModel.SelectedTelegramUserIds != null && groupViewModel.SelectedTelegramUserIds.Any())
                {
                    var userGroups = groupViewModel.SelectedTelegramUserIds.Select(userId => new UserGroup
                    {
                        GroupId = group.Id,
                        TelegramUserId = userId
                    }).ToList();

                    await _userGroupsService.AddRangeUserGroups(userGroups);
                }

                await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.Group, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Medium, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} created group {groupViewModel.Name}" });

                return RedirectToAction("Index");
            }

            await PopulateGroupViewModel(groupViewModel); 
            return View(groupViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var group = await _groupsService.GetGroupById(id);

            if (group is null)
            {
                TempData[TempDataKeys.GroupNotFound] = true;
                return RedirectToAction("Index");
            }

            var telegramUsers = await _telegramUsersService.GetAllTelegramUsers();

            var groupViewModel = new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name,
                SelectedTelegramUserIds = group.UserGroups?.Select(ug => ug.TelegramUserId).ToList() ?? new List<int>(),
                AvailableTelegramUsers = telegramUsers.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name + " " + p.Surname }).ToList()
            };

            return View(groupViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroupViewModel groupViewModel)
        {
            if (ModelState.IsValid)
            {
                var group = await _groupsService.GetGroupById(id);

                if (group is null)
                {
                    TempData[TempDataKeys.GroupNotFound] = true;
                    return RedirectToAction("Index");
                }

                group.Name = groupViewModel.Name;
                group.UpdatedDate = DateTime.Now;

                // Update UserGroups
                group.UserGroups = groupViewModel.SelectedTelegramUserIds != null && groupViewModel.SelectedTelegramUserIds.Any()
                    ? groupViewModel.SelectedTelegramUserIds.Select(userId => new UserGroup
                    {
                        GroupId = group.Id,
                        TelegramUserId = userId
                    }).ToList()
                    : null;

                await _groupsService.UpdateGroup(group);
                await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.Group, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Medium, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} updated group {groupViewModel.Name}" });

                return RedirectToAction("Index");
            }

            await PopulateGroupViewModel(groupViewModel);
            return View(groupViewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
           var group = await _groupsService.GetGroupById(id);

           if (group is not null)
               return View(group);

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int id)
        {
            if (ModelState.IsValid)
            {
                var group = await _groupsService.GetGroupById(id);

                if (group is not null)
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.Group, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Higer, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} deleted group {group.Name}" });
                    await _groupsService.DeleteGroup(group);
                }
            }

            return RedirectToAction("Index");
        }

        // Helper: Re-populate available lists if model state is invalid.
        private async Task PopulateGroupViewModel(GroupViewModel model)
        {
            var telegramUsers = await _telegramUsersService.GetAllTelegramUsers();
            model.AvailableTelegramUsers = telegramUsers.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name + " " + p.Surname
            }).ToList();
        }
    }
}
