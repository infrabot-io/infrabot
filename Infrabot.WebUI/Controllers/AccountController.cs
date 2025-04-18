using Infrabot.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Infrabot.WebUI.Models;
using Infrabot.WebUI.Services;
using Infrabot.WebUI.Constants;

namespace Infrabot.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger; 
        private readonly IAuditLogService _auditLogService;
        private readonly SignInManager<User> _signManager;
        private readonly UserManager<User> _userManager;

        public AccountController(
            ILogger<AccountController> logger, 
            IAuditLogService auditLogService, 
            UserManager<User> userManager, 
            SignInManager<User> signManager)
        {
            _logger = logger;
            _auditLogService = auditLogService;
            _signManager = signManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            var model = new LoginViewModel { };
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _signManager.UserManager.FindByNameAsync(model.UserName);

                if (user == null)
                {
                    ViewData[TempDataKeys.AccountLoginOrPasswordIncorrect] = true;
                    return View(model);
                }

                if (user.Enabled == false)
                {
                    ViewData[TempDataKeys.AccountLoginDenied] = true;
                    return View(model);
                }

                var result = await _signManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.LogIn, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success,LogSeverity = AuditLogSeverity.Medium, CreatedDate = DateTime.Now, Description = $"User with user name {user.UserName} logged in" });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.LogIn,LogItem = AuditLogItem.User, LogResult = AuditLogResult.Failure, LogSeverity = AuditLogSeverity.Higer, CreatedDate = DateTime.Now, Description = $"User with user name failed to {user.UserName} log in"});
                    ViewData[TempDataKeys.AccountLoginOrPasswordIncorrect] = true;
                }
            }

            await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.LogIn, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Error, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"Log in form failure. Wrong data." });

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.LogOut, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Medium, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} logged out" });
            await _signManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel { };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if(model.NewPassword != model.NewPasswordRepeat)
                {
                    ViewData[TempDataKeys.AccountNewPasswordNotEqualToRepeat] = true;
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(this.User);

                if (user == null)
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.NotFound, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} failed to change a password, because user was not found in the database. This is unusual"});
                    ViewData[TempDataKeys.AccountUserNotFound] = true;
                    return View(model);
                }

                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} changed password" });
                    ViewData[TempDataKeys.AccountPasswordChangeFailed] = true;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Failure, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} could not change password" });
                    ViewData[TempDataKeys.AccountPasswordChangeFailed] = true;
                }
            }

            await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Error, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} could not change password. Form failure. Wrong data." });
            
            return View(model);
        }
    }
}
