using Infrabot.WebUI.Services;
using Infrabot.WebUI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrabot.WebUI.Controllers
{
    [Authorize]
    public class LogsController : Controller
    {
        private readonly ILogger<LogsController> _logger;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;
        private string logsFilePath;

        public LogsController(
            ILogger<LogsController> logger,
            IAuditLogService auditLogService,
            IConfiguration configuration)
        {
            _logger = logger;
            _auditLogService = auditLogService;
            _configuration = configuration;

            logsFilePath =  PathNormalizer.NormalizePath(_configuration["Serilog:WriteTo:2:Args:path"].Replace(".log", DateTime.Now.ToString("yyyyMMdd") + ".log") ?? "logs");

            _logger.LogInformation(logsFilePath);
        }

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
