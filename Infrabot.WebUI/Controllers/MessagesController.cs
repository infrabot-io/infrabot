using Infrabot.Common.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Controllers
{
    public class MessagesController : Controller
    {

        private readonly InfrabotContext _context;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ILogger<MessagesController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var telegramMessages = await _context.TelegramMessages.OrderByDescending(x => x.CreatedDate).Take(50).ToListAsync();
            return View(telegramMessages);
        }
    }
}
