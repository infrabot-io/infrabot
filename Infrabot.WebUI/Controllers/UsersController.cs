﻿using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using infrabot.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.WebUI.Services;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infrabot.WebUI.Constants;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUsersService _userService;
        private readonly IAuditLogsService _auditLogsService;
        private readonly UserManager<User> _userManager;

        public UsersController(
            ILogger<HomeController> logger,
            IUsersService userService,
            IAuditLogsService auditLogsService,
            UserManager<User> userManager)
        {
            _logger = logger;
            _userService = userService;
            _auditLogsService = auditLogsService;
            _userManager = userManager;
        }
        
        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;
            var count = await _userService.GetUsersCount() - 1;
            var users = await _userService.GetUsers(page, pageSize);
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new UserViewModel { };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userCheckName = await _userManager.FindByNameAsync(model.UserName);
                if (userCheckName != null) 
                {  
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Denied, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} was not able to create user with username {model.UserName} because it already exists" });
                    ViewData[TempDataKeys.CreateUserAlreadyExists] = true;
                    return View(model); 
                }

                var userCheckEmail = await _userManager.FindByEmailAsync(model.Email);
                if (userCheckEmail != null) 
                { 
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Denied, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} was not able to create user with email {model.Email} because it already exists" });
                    ViewData[TempDataKeys.CreateUserAlreadyExists] = true;
                    return View(model); 
                }

                var user = new User() 
                { 
                    Name = model.Name,
                    Surname = model.Surname,
                    UserName = model.UserName,
                    NormalizedUserName = model.UserName,
                    Email = model.Email, 
                    NormalizedEmail = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Enabled = model.Enabled,
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    ViewData[TempDataKeys.CreateUserSucceeded] = true;
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} created user with username {user.UserName} and email {user.Email}" });
                }
                else
                {
                    ViewData[TempDataKeys.CreateUserFailed] = true;
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Failure, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} was not able to create user with username {user.UserName} and email {user.Email}" });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> View(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return RedirectToAction("Index");
            
            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return RedirectToAction("Index");

            var model = new UserViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Enabled = user.Enabled
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user is null)
                    return RedirectToAction("Index");

                if(!user.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase))
                { 
                    var userCheckName = await _userManager.FindByNameAsync(model.UserName);
                    if (userCheckName != null)
                    {
                        await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Denied, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} was not able to edit and set {model.UserName} value to user with username {user.UserName} because it already exists." });
                        ViewData[TempDataKeys.CreateUserAlreadyExists] = true;
                        return View(model);
                    }

                    var userCheckEmail = await _userManager.FindByEmailAsync(model.Email);
                    if (userCheckEmail != null)
                    {
                        await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Denied, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} was not able to edit and set {model.Email} value to user with email {user.Email} because it already exists." });
                        ViewData[TempDataKeys.CreateUserAlreadyExists] = true;
                        return View(model);
                    }
                }

                user.Name = model.Name;
                user.Surname = model.Surname;
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Enabled = model.Enabled;

                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} updated user {user.UserName}." });
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} updated password of user {user.UserName}." });
                    
                    await _userManager.RemovePasswordAsync(user);
                    var result = await _userManager.AddPasswordAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        ViewData[TempDataKeys.EditUserSuccessWithPassword] = true;
                    }
                    else
                    {
                        ViewData[TempDataKeys.EditUserFailedWithPassword] = true;
                    }
                }
            }

            ViewData[TempDataKeys.EditUserSuccess] = true;

            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is not null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(string id)
        {
            if (ModelState.IsValid)
            {
                int usersCount = await _userManager.Users.CountAsync();

                if (usersCount == 1)
                    return RedirectToAction("Index");

                var user = await _userManager.FindByIdAsync(id);

                if (user is not null)
                {
                    await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} deleted user with username {user.UserName} and email {user.Email}" });
                    await _userManager.DeleteAsync(user);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
