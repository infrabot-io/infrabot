using Infrabot.Common.Enums;
using Infrabot.Common.Models;
using infrabot.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.WebUI.Services;
using Infrabot.WebUI.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUsersService _userService;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<User> _userManager;

        public UsersController(ILogger<HomeController> logger, IUsersService userService, IAuditLogService auditLogService, UserManager<User> userManager)
        {
            _logger = logger;
            _userService = userService;
            _auditLogService = auditLogService;
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
            var model = new CreateUserViewModel { };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userCheckName = await _userManager.FindByNameAsync(model.UserName);
                if (userCheckName != null) {  model.UserAlreadyExists = true;  return View(model); }
                var userCheckEmail = await _userManager.FindByEmailAsync(model.Email);
                if (userCheckEmail != null) { model.UserAlreadyExists = true; return View(model); }

                var user = new User() 
                { 
                    Name = model.Name,
                    Surname = model.Surname,
                    UserName = model.UserName,
                    NormalizedUserName = model.UserName,
                    Email = model.Email, 
                    NormalizedEmail = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    IsADIntegrated = model.IsADIntegrated,
                    Enabled = model.Enabled,
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    model.UserCreationSucceeded = true;
                    await _auditLogService.AddAuditLog(new AuditLog { LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"Created user {user.UserName} by {this.User}" });
                }
                else
                {
                    model.UserCreationSucceeded = false;
                    await _auditLogService.AddAuditLog(new AuditLog { LogAction = AuditLogAction.Create, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"Created user {user.UserName} by {this.User} failed" });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> View(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user is not null)
                return View(user);

            return RedirectToAction("Index");
        }

        /*

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

        */
    }
}
