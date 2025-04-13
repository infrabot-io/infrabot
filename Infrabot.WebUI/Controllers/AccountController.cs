using Infrabot.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrabot.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Infrabot.WebUI.Models;
using Infrabot.WebUI.Services;

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

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _signManager.UserManager.FindByNameAsync(model.UserName);

                if (user == null)
                {
                    model.LoginOrPasswordIncorrect = true;
                    return View(model);
                }

                if (user.Enabled == false)
                {
                    model.LoginDenied = true;
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
                    model.LoginOrPasswordIncorrect = true;
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
            await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.LogOut, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Medium, CreatedDate = DateTime.Now, Description = $"User {this.User} logged out" });
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
                    model.NewPasswordNotEqualToRepeat = true;
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(this.User);

                if (user == null)
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.NotFound, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User} failed to change a password, because user was not found in the database. This is unusual"});
                    return View(model);
                }

                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Success, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User} changed password" });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Failure, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User} could not change password" });

                    //return RedirectToAction("LogOut", "Account");
                }
            }

            await _auditLogService.AddAuditLog(new AuditLog { IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(), LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, LogResult = AuditLogResult.Error, LogSeverity = AuditLogSeverity.Highest, CreatedDate = DateTime.Now, Description = $"User {this.User} could not change password. Form failure. Wrong data." });
            
            return View(model);
        }

        /*
        public IActionResult LogIn()
        {
            ViewBag.LoginOrPasswordIncorrect = TempData[TempDataKeys.LoginOrPasswordIncorrect];
            ViewBag.LoginDenied = TempData[TempDataKeys.LoginDenied];
            ViewBag.LoginDataIsNotValid = TempData[TempDataKeys.LoginDataIsNotValid];
            ViewBag.ADAuthFailed = TempData[TempDataKeys.ADAuthFailed];
            ViewBag.LoginDeniedForApiUser = TempData[TempDataKeys.LoginDeniedForApiUser];
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
                    TempData[TempDataKeys.LoginDataIsNotValid] = true;
                    return RedirectToAction("LogIn");
                }

                if (_user.Enabled == false)
                {
                    _logger.LogInformation("User is disabled");
                    TempData[TempDataKeys.LoginDenied] = true;
                    return RedirectToAction("LogIn");
                }

                if (configuration.IsADEnabled && _user.IsADIntegrated == true)
                {
                    ActiveDirectoryAuthenticator activeDirectoryAuthenticator = new ActiveDirectoryAuthenticator(configuration.ADServer, configuration.ADDomainName, configuration.ADLogin, configuration.ADPassword);
                    bool isAuthenticated = activeDirectoryAuthenticator.Authenticate(user.Login, user.Password);

                    if (isAuthenticated == false)
                    {
                        _logger.LogInformation("AD authentication failed");
                        TempData[TempDataKeys.ADAuthFailed] = true;
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
                    TempData[TempDataKeys.LoginOrPasswordIncorrect] = true;
                    return RedirectToAction("LogIn");
                }

                var claims = new List<Claim>
                {
                    new Claim (CustomClaimTypes.UserId, _user.Id.ToString()),
                    new Claim (ClaimTypes.Name, _user.Login),
                    new Claim (CustomClaimTypes.Login, _user.Login)
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

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.LogIn, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {_user.Login} logged in" });

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData[TempDataKeys.LoginDataIsNotValid] = true;
                return RedirectToAction("LogIn");
            }
            
            return RedirectToAction("LogIn");
        }

        public async Task<IActionResult> LogOut()
        {
            _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.LogOut, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {HttpContext.User.FindFirstValue("Login")} logged out" });
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

            ViewBag.OldPasswordIsIncorrect = TempData[TempDataKeys.OldPasswordIsIncorrect];
            ViewBag.NewPasswordNotEqualToRepeat = TempData[TempDataKeys.NewPasswordNotEqualToRepeat];
            ViewBag.DoesNotMeetComplexityRequirements = TempData[TempDataKeys.DoesNotMeetComplexityRequirements];
            ViewBag.SucessfullyChanged = TempData[TempDataKeys.SucessfullyChanged];
            
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
                    TempData[TempDataKeys.OldPasswordIsIncorrect] = true;
                    return RedirectToAction("ChangePassword");
                }

                if (NewPassword != RepeatPassword)
                {
                    TempData[TempDataKeys.NewPasswordNotEqualToRepeat] = true;
                    return RedirectToAction("ChangePassword");
                }

                if (!PasswordPolicyChecker.CheckPasswordForPolicy(NewPassword, configuration.PasswordPolicyMinLength, configuration.PasswordPolicyContainSpecialCharacter, configuration.PasswordPolicyContainNumber, configuration.PasswordPolicyContainLowerCase, configuration.PasswordPolicyContainUpperCase))
                {
                    TempData[TempDataKeys.DoesNotMeetComplexityRequirements] = true;
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

                _context.AuditLogs.Add(new AuditLog { LogAction = AuditLogAction.ChangePassword, LogItem = AuditLogItem.User, CreatedDate = DateTime.Now, Description = $"User {_user.Login} changed password" });

                await _context.SaveChangesAsync();
            }

            TempData[TempDataKeys.SucessfullyChanged] = true;
            
            return RedirectToAction("ChangePassword");
        }
    */
    }
}
