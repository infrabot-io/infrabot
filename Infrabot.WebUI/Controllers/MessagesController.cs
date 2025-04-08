using Infrabot.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly ITelegramMessagesService _telegramMessagesService;

        public MessagesController(
            ILogger<MessagesController> logger,
            ITelegramMessagesService telegramMessagesService)
        {
            _logger = logger;
            _telegramMessagesService = telegramMessagesService;
        }

        public async Task<IActionResult> Index()
        {
            var telegramMessages = await _telegramMessagesService.GetTelegramMessages();
            return View(telegramMessages);
        }
    }
}
