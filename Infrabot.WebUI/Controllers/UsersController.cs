using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using infrabot.Controllers;
using Infrabot.Common.Contexts;
using Infrabot.WebUI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public UsersController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        
        [Authorize]
        public async Task<IActionResult> Index(int page = 0)
        {
            int pageSize = 50;
            var count = await _userService.GetUsersCount();
            var users = await _userService.GetUsers(page, pageSize);
            var maxpage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);

            ViewBag.MaxPage = maxpage;
            ViewBag.Page = page;
            ViewBag.Pages = maxpage + 1;

            return View(users);
        }

        /*
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.ConfigurationError = TempData[TempDataKeys.ConfigurationError];
            ViewBag.UserAlreadyExists = TempData[TempDataKeys.UserAlreadyExists];
            ViewBag.PasswordDoesNotMeetComplexity = TempData[TempDataKeys.PasswordDoesNotMeetComplexity];
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            var configuration = await _context.Configurations.FirstOrDefaultAsync(s => s.Id == 1);
            User? _user = _context.Users.Where(x => (x.Login == user.Login) || (x.Email == user.Email)).FirstOrDefault();

            if (configuration is null)
            {
                TempData[TempDataKeys.ConfigurationError] = true;
                return RedirectToAction("Create", user);
            }

            if (_user is not null)
            {
                TempData[TempDataKeys.UserAlreadyExists] = true;
                return RedirectToAction("Create", user);
            }

            if (user.Password == null)
                user.Password = string.Empty;
            if (user.Phone == null)
                user.Phone = string.Empty;

            if (!PasswordPolicyChecker.CheckPasswordForPolicy(user.Password, configuration.PasswordPolicyMinLength, configuration.PasswordPolicyContainSpecialCharacter, configuration.PasswordPolicyContainNumber, configuration.PasswordPolicyContainLowerCase, configuration.PasswordPolicyContainUpperCase))
            {
                TempData[TempDataKeys.PasswordDoesNotMeetComplexity] = true;
                return RedirectToAction("Create", user);
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} created user '{user.Login}'" });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == Id);
            if (user is not null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, User user)
        {
            var _user = await _context.Users.FirstOrDefaultAsync(s => s.Id == Id);

            if (_user is null)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                _user.Email = user.Email;
                _user.Name = user.Name;
                _user.Surname = user.Surname;
                _user.IsADIntegrated = user.IsADIntegrated;
                _user.Enabled = user.Enabled;
                _user.Login = user.Login;
                _user.Phone = user.Phone;
                _user.Password = user.Password;
                _user.Email = user.Email;

                _context.Update(_user);

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Update, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} updated user '{user.Login}'" });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == Id);
            if (user is not null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePressed(int Id)
        {
            var user = await _context.Users.FindAsync(Id);

            if (user is not null)
            {
                _context.Users.Remove(user);
                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Delete, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} deleted user '{user.Login}'" });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> View(int Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == Id);
            if (user is not null)
                return View(user);
            else
                return RedirectToAction("Index");
        }
        */
    }
}
