using Infrabot.Common.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    public class LogsController : Controller
    {
        //private static readonly string logsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs\\" + $"application{DateTime.Now.ToString("yyyyMMdd")}.log");
        private readonly string logsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs\\" + $"application.log");
        private readonly InfrabotContext _context;
        private readonly ILogger<LogsController> _logger;

        public LogsController(ILogger<LogsController> logger, InfrabotContext infrabotContext)
        {
            _logger = logger;
            _context = infrabotContext;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.Logs = await ReadLastLines(logsFilePath, 500);

            return View();
        }

        private async Task<string> ReadLastLines(string filePath, int lineCount)
        {
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"The specified file does not exist {filePath}");
                return string.Empty;
            }

            var lines = new LinkedList<string>();
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line != null)
                        {
                            lines.AddLast(line);
                            if (lines.Count > lineCount)
                                lines.RemoveFirst(); // Keep only the last `lineCount` lines
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading file: {ex.Message}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
