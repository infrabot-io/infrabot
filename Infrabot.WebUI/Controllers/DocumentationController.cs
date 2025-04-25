using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class DocumentationController : Controller
    {
        private readonly ILogger<DocumentationController> _logger;

        public DocumentationController(ILogger<DocumentationController> logger)
        {
            _logger = logger;
        }

        public IActionResult Introduction()
        {
            return View();
        }

        public IActionResult Contents()
        {
            return View();
        }

        public IActionResult GettingStarted()
        {
            return View();
        }

        public IActionResult Examples()
        {
            return View();
        }

        public IActionResult AnswersToQuestions()
        {
            return View();
        }
    }
}
