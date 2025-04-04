using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infrabot.Common.Contexts;
using Infrabot.WebUI.Constants;

namespace Infrabot.WebUI.Controllers
{
    public class GroupsController : Controller
    {
        private readonly InfrabotContext _context;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(ILogger<GroupsController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 0)
        {
            const int PageSize = 50;

            var count = _context.Groups.Count() - 1;
            var users = await _context.Groups.OrderBy(s => s.Name).Skip(page * PageSize).Take(PageSize).ToListAsync();
            var maxpage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(users);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var groupViewModel = new GroupViewModel
            {
                AvailableTelegramUsers = await _context.TelegramUsers.Select(p => new SelectListItem{ Value = p.Id.ToString(), Text = p.Name + " " + p.Surname}).ToListAsync()
            };

            ViewBag.GroupAlreadyExists = TempData[TempDataKeys.GroupAlreadyExists];
            return View(groupViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel groupViewModel)
        {
            var existingGroup = await _context.Groups.FirstOrDefaultAsync(x => x.Name.ToLower() == groupViewModel.Name.ToLower());

            if (existingGroup != null)
            {
                TempData[TempDataKeys.GroupAlreadyExists] = true;
                return RedirectToAction("Create", existingGroup);
            }

            if (ModelState.IsValid)
            {
                var group = new Group
                {
                    Name = groupViewModel.Name,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();

                if (groupViewModel.SelectedTelegramUserIds != null && groupViewModel.SelectedTelegramUserIds.Any())
                {
                    var userGroups = groupViewModel.SelectedTelegramUserIds.Select(userId => new UserGroup
                    {
                        GroupId = group.Id,
                        TelegramUserId = userId
                    }).ToList();
                    await _context.UserGroups.AddRangeAsync(userGroups);
                    await _context.SaveChangesAsync();
                }

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Create, LogItem = AuditLogItem.Group, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} created group '{group.Name}'" });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            PopulateGroupViewModel(groupViewModel); 
            return View(groupViewModel);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int Id)
        {
            var group = await _context.Groups.Include(x => x.UserGroups).FirstOrDefaultAsync(s => s.Id == Id);

            if (group is null)
                return RedirectToAction("Index");

            var groupViewModel = new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name,
                SelectedTelegramUserIds = group.UserGroups?.Select(ug => ug.TelegramUserId).ToList() ?? new List<int>(),
                AvailableTelegramUsers = await _context.TelegramUsers.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name + " " + p.Surname }).ToListAsync()
            };

            return View(groupViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, GroupViewModel groupViewModel)
        {
            if (ModelState.IsValid)
            {
                Group? group = await _context.Groups.Include(x => x.UserGroups).FirstOrDefaultAsync(s => s.Id == Id);

                if (group is null)
                    return RedirectToAction("Index");

                group.Name = groupViewModel.Name;
                group.UpdatedDate = DateTime.Now;
                _context.Entry(group).State = EntityState.Modified;
                _context.Entry(group).Property(p => p.CreatedDate).IsModified = false;

                // Update UserGroups
                group.UserGroups = groupViewModel.SelectedTelegramUserIds != null && groupViewModel.SelectedTelegramUserIds.Any()
                    ? groupViewModel.SelectedTelegramUserIds.Select(userId => new UserGroup
                    {
                        GroupId = group.Id,
                        TelegramUserId = userId
                    }).ToList()
                    : null;

                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Update, LogItem = AuditLogItem.Group, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} modified group '{group.Name}'" });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            PopulateGroupViewModel(groupViewModel);
            return View(groupViewModel);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(s => s.Id == Id);
            if (group is not null)
                return View(group);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int Id)
        {
            var group = await _context.Groups.FindAsync(Id);

            if (group != null)
            {
                _context.Groups.Remove(group);
                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.Group, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} deleted group '{group.Name}'" });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Helper: Re-populate available lists if model state is invalid.
        private void PopulateGroupViewModel(GroupViewModel model)
        {
            model.AvailableTelegramUsers = _context.TelegramUsers.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name + " " + p.Surname
            }).ToList();
        }
    }
}
