using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using infrabot.Controllers;
using Infrabot.Common.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Infrabot.WebUI.Constants;

namespace Infrabot.WebUI.Controllers
{
    public class TelegramUsersController : Controller
    {
        private readonly InfrabotContext _context;
        private readonly ILogger<HomeController> _logger;

        public TelegramUsersController(ILogger<HomeController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 0)
        {
            const int PageSize = 50;

            var count = _context.TelegramUsers.Count() - 1;
            var telegramUsers = await _context.TelegramUsers.OrderBy(s => s.Name).Skip(page * PageSize).Take(PageSize).ToListAsync();
            var maxpage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(telegramUsers);
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.UserAlreadyExists = TempData[TempDataKeys.TelegramUserAlreadyExists];
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TelegramUser telegramUser)
        {
            TelegramUser _telegramUser = _context.TelegramUsers.Where(x => x.TelegramId == telegramUser.TelegramId).FirstOrDefault();

            if (_telegramUser != null)
            {
                TempData[TempDataKeys.TelegramUserAlreadyExists] = true;
                return RedirectToAction("Create", telegramUser);
            }

            if (ModelState.IsValid)
            {
                _context.TelegramUsers.Add(telegramUser);
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Create, LogItem = AuditLogItem.TelegramUser, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} created telegram user '{telegramUser.Name} {telegramUser.Surname}'" });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(telegramUser);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int Id)
        {
            var telegramUser = await _context.TelegramUsers.FindAsync(Id);
            if (telegramUser is not null)
                return View(telegramUser);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, TelegramUser telegramUser)
        {
            var _telegramUser = await _context.TelegramUsers.FirstOrDefaultAsync(s => s.Id == Id);

            if (_telegramUser is null)
                return View(telegramUser);

            if (ModelState.IsValid)
            {
                _telegramUser.Name = telegramUser.Name;
                _telegramUser.Surname = telegramUser.Surname;
                _telegramUser.TelegramId = telegramUser.TelegramId;

                _context.Update(_telegramUser);

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Update, LogItem = AuditLogItem.TelegramUser, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} modified telegram user '{telegramUser.Name} {telegramUser.Surname}'" });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(telegramUser);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            var telegramUser = await _context.TelegramUsers.FirstOrDefaultAsync(s => s.Id == Id);
            if (telegramUser is not null)
                return View(telegramUser);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int Id)
        {
            var telegramUser = await _context.TelegramUsers.FindAsync(Id);
            if (telegramUser != null)
            {
                _context.TelegramUsers.Remove(telegramUser);
                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.TelegramUser, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} deleted telegram user '{telegramUser.Name} {telegramUser.Surname}'" });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
