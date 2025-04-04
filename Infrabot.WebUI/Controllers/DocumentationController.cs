using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    public class DocumentationController : Controller
    {
        private readonly ILogger<DocumentationController> _logger;

        public DocumentationController(ILogger<DocumentationController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public IActionResult Introduction()
        {
            return View();
        }

        [Authorize]
        public IActionResult Contents()
        {
            return View();
        }

        [Authorize]
        public IActionResult GettingStarted()
        {
            return View();
        }

        [Authorize]
        public IActionResult Examples()
        {
            return View();
        }
    }
}
