using Infrabot.Common.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infrabot.Common.Enums;
using Infrabot.WebUI.Utils;
using Infrabot.Common.Contexts;

namespace Infrabot.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly InfrabotContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, InfrabotContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult LogIn()
        {
            ViewBag.LoginOrPasswordIncorrect = TempData["LoginOrPasswordIncorrect"];
            ViewBag.LoginDenied = TempData["LoginDenied"];
            ViewBag.LoginDataIsNotValid = TempData["LoginDataIsNotValid"];
            ViewBag.ADAuthFailed = TempData["ADAuthFailed"];
            ViewBag.LoginDeniedForApiUser = TempData["LoginDeniedForApiUser"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn([Bind("Login, Password")] User user)
        {
            Configuration? configuration = await _context.Configurations.FirstOrDefaultAsync(s => s.Id == 1);
            if (configuration == null) return RedirectToAction("LogIn");

            if (ModelState.IsValid)
            {
                User? _user = await _context.Users.Where(x => x.Login.ToLower() == user.Login.ToLower()).FirstOrDefaultAsync();

                if (_user == null)
                {
                    _logger.LogInformation("User was not found. Only login check.");
                    TempData["LoginDataIsNotValid"] = true;
                    return RedirectToAction("LogIn");
                }

                if (_user.Enabled == false)
                {
                    _logger.LogInformation("User is disabled");
                    TempData["LoginDenied"] = true;
                    return RedirectToAction("LogIn");
                }

                if (configuration.IsADEnabled && _user.IsADIntegrated == true)
                {
                    ActiveDirectoryAuthenticator activeDirectoryAuthenticator = new ActiveDirectoryAuthenticator(configuration.ADServer, configuration.ADDomainName, configuration.ADLogin, configuration.ADPassword);
                    bool isAuthenticated = activeDirectoryAuthenticator.Authenticate(user.Login, user.Password);

                    if (isAuthenticated == false)
                    {
                        _logger.LogInformation("AD authentication failed");
                        TempData["ADAuthFailed"] = true;
                        return RedirectToAction("LogIn");
                    }
                }
                else
                {
                    _user = _context.Users.Where(x => x.Login.ToLower() == user.Login.ToLower() && x.Password == user.Password).FirstOrDefault();
                }

                if (_user == null)
                {
                    _logger.LogInformation("User was not found. Password check");
                    TempData["LoginOrPasswordIncorrect"] = true;
                    return RedirectToAction("LogIn");
                }

                var claims = new List<Claim>
                {
                    new Claim ("UserId", _user.Id.ToString()),
                    new Claim (ClaimTypes.Name, _user.Login),
                    new Claim ("Login", _user.Login)
                };

                _context.Entry(_user).State = EntityState.Modified;
                _context.Entry(_user).Property(p => p.Id).IsModified = false;
                _context.Entry(_user).Property(p => p.Name).IsModified = false;
                _context.Entry(_user).Property(p => p.Surname).IsModified = false;
                _context.Entry(_user).Property(p => p.Login).IsModified = false;
                _context.Entry(_user).Property(p => p.Email).IsModified = false;
                _context.Entry(_user).Property(p => p.Password).IsModified = false;
                _context.Entry(_user).Property(p => p.Phone).IsModified = false;
                _context.Entry(_user).Property(p => p.IsADIntegrated).IsModified = false;
                _context.Entry(_user).Property(p => p.Enabled).IsModified = false;
                _context.Entry(_user).Property(p => p.CreatedDate).IsModified = false;
                _user.LastLoginDate = DateTime.Now;
                _user.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["LoginDataIsNotValid"] = true;
                return RedirectToAction("LogIn");
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LogIn", "Account");
        }

        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            int UserID = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));
            string UserLogin = User.Identity.Name;

            if(UserLogin is null) 
            {
                _logger.LogInformation($"User login was not found in cookies. Force log out user.");
                return RedirectToAction("LogOut", "Account");
            }

            User? _user = await _context.Users.Where(x => (x.Id == UserID) && (x.Login.ToLower() == UserLogin.ToLower())).FirstOrDefaultAsync();
            
            if (_user == null) return RedirectToAction("LogOut");
            if (_user.IsADIntegrated) return RedirectToAction("Index", "Home");

            ViewBag.OldPasswordIsIncorrect = TempData["OldPasswordIsIncorrect"];
            ViewBag.NewPasswordNotEqualToRepeat = TempData["NewPasswordNotEqualToRepeat"];
            ViewBag.DoesNotMeetComplexityRequirements = TempData["DoesNotMeetComplexityRequirements"];
            ViewBag.SucessfullyChanged = TempData["SucessfullyChanged"];

            return View("ChangePassword", _user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword, string RepeatPassword, [Bind("Id, Login")] User user)
        {
            string UserLogin = "";
            int UserID = 0;

            if (ModelState.IsValid)
            {
                var configuration = await _context.Configurations.FirstOrDefaultAsync(s => s.Id == 1);
                if (configuration == null)
                {
                    _logger.LogError("System configuration is empty in the database. Please fix the system first");
                    return RedirectToAction("LogOut", "Account");
                }

                if (HttpContext.User.FindFirstValue("UserId") != null)
                    UserID = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));
                else
                {
                    _logger.LogError("User was not found from the context");
                    return RedirectToAction("LogOut", "Account");
                }

                if (User.Identity.Name != null)
                    UserLogin = User.Identity.Name;
                else
                {
                    _logger.LogInformation("User data is empty");
                    return RedirectToAction("LogOut", "Account");
                }

                var _user = _context.Users.Where(x => (x.Id == UserID) && (x.Login.ToLower() == UserLogin.ToLower())).FirstOrDefault();

                if (_user == null)
                {
                    _logger.LogInformation("User data was not found");
                    return RedirectToAction("LogOut", "Account");
                }

                if (_user.Password != OldPassword)
                {
                    TempData["OldPasswordIsIncorrect"] = true;
                    return RedirectToAction("ChangePassword");
                }

                if (NewPassword != RepeatPassword)
                {
                    TempData["NewPasswordNotEqualToRepeat"] = true;
                    return RedirectToAction("ChangePassword");
                }

                if (!PasswordPolicyChecker.CheckPasswordForPolicy(NewPassword, configuration.PasswordPolicyMinLength, configuration.PasswordPolicyContainSpecialCharacter, configuration.PasswordPolicyContainNumber, configuration.PasswordPolicyContainLowerCase, configuration.PasswordPolicyContainUpperCase))
                {
                    TempData["DoesNotMeetComplexityRequirements"] = true;
                    return RedirectToAction("ChangePassword");
                }

                _context.ChangeTracker.Clear();
                _context.Entry(user).State = EntityState.Modified;
                _context.Entry(user).Property(p => p.Id).IsModified = false;
                _context.Entry(user).Property(p => p.CreatedDate).IsModified = false;
                _context.Entry(user).Property(p => p.Surname).IsModified = false;
                _context.Entry(user).Property(p => p.Name).IsModified = false;
                _context.Entry(user).Property(p => p.UpdatedDate).IsModified = false;
                _context.Entry(user).Property(p => p.IsADIntegrated).IsModified = false;
                _context.Entry(user).Property(p => p.Email).IsModified = false;
                _context.Entry(user).Property(p => p.Enabled).IsModified = false;
                _context.Entry(user).Property(p => p.Phone).IsModified = false;
                _context.Entry(user).Property(p => p.Login).IsModified = false;
                _context.Entry(user).Property(p => p.LastLoginDate).IsModified = false;
                user.Password = NewPassword;
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.Update, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {_user.Login} changed password" });

                await _context.SaveChangesAsync();
            }

            TempData["SucessfullyChanged"] = true;

            return RedirectToAction("ChangePassword");
        }
    }
}
