using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using infrabot.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class TelegramUsersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITelegramUsersService _telegramUsersService;
        private readonly IAuditLogsService _auditLogsService;

        public TelegramUsersController(
            ILogger<HomeController> logger, 
            ITelegramUsersService telegramUsersService, 
            IAuditLogsService auditLogsService)
        {
            _logger = logger;
            _telegramUsersService = telegramUsersService;
            _auditLogsService = auditLogsService;
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;
            var count = await _telegramUsersService.GetTelegramUsersCount() - 1;
            var telegramUsers = await _telegramUsersService.GetTelegramUsers(page, pageSize);
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            ViewBag.TelegramUserNotFound = TempData[TempDataKeys.TelegramUserNotFound] as bool?;
            ViewBag.TelegramUserDeleted = TempData[TempDataKeys.TelegramUserDeleted] as bool?;

            return View(telegramUsers);
        }

        public IActionResult Create()
        {
            ViewBag.UserAlreadyExists = TempData[TempDataKeys.TelegramUserAlreadyExists];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TelegramUser telegramUser)
        {
            if (ModelState.IsValid)
            {
                TelegramUser _telegramUser = await _telegramUsersService.GetTelegramUserByTelegramId(telegramUser.TelegramId);

                if (_telegramUser != null)
                {
                    TempData[TempDataKeys.TelegramUserAlreadyExists] = true;
                    return RedirectToAction("Create", telegramUser);
                }

                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Create, LogItem = AuditLogItem.TelegramUser, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} created telegram user {telegramUser.Name} {telegramUser.Surname} with id {telegramUser.TelegramId}" });
                await _telegramUsersService.CreateTelegramUser(telegramUser);

                return RedirectToAction("Index");
            }

            return View(telegramUser);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var telegramUser = await _telegramUsersService.GetTelegramUserById(id);

            if (telegramUser is null)
            {
                TempData[TempDataKeys.TelegramUserNotFound] = true;
                return RedirectToAction("Index");
            }

            return View(telegramUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TelegramUser telegramUser)
        {
            if (ModelState.IsValid)
            {
                var _telegramUser = await _telegramUsersService.GetTelegramUserById(id);

                if (_telegramUser is null)
                {
                    TempData[TempDataKeys.TelegramUserNotFound] = true;
                    return View(telegramUser);
                }

                _telegramUser.Name = telegramUser.Name;
                _telegramUser.Surname = telegramUser.Surname;
                _telegramUser.TelegramId = telegramUser.TelegramId;

                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Update, LogItem = AuditLogItem.TelegramUser, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} updated telegram user and set {telegramUser.Name} {telegramUser.Surname} with id {telegramUser.TelegramId}" });
                await _telegramUsersService.UpdateTelegramUser(_telegramUser);

                ViewData[TempDataKeys.TelegramUserSaved] = true;
            }

            return View(telegramUser);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var telegramUser = await _telegramUsersService.GetTelegramUserById(id);

            if (telegramUser is null)
            {
                TempData[TempDataKeys.TelegramUserNotFound] = true;
                return RedirectToAction("Index");
            }

            return View(telegramUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int id)
        {
            if (ModelState.IsValid)
            {
                var telegramUser = await _telegramUsersService.GetTelegramUserById(id);

                if (telegramUser is null)
                {
                    TempData[TempDataKeys.TelegramUserNotFound] = true;
                    return RedirectToAction("Index");
                }

                await _auditLogsService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.TelegramUser, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User.Identity?.Name} deleted telegram user {telegramUser.Name} {telegramUser.Surname} with id {telegramUser.TelegramId}" });
                await _telegramUsersService.DeleteTelegramUser(telegramUser);
                TempData[TempDataKeys.TelegramUserDeleted] = true;
            }

            return RedirectToAction("Index");
        }
    }
}
