namespace Infrabot.Common.Models
{
    public class Configuration
    {
        public int Id { get; set; }
        public string? TelegramBotToken { get; set; } = string.Empty;
        public bool TelegramEnableEmergency { get; set; } = true;
        public bool TelegramEnableShowMyId { get; set; } = true;
        public string? TelegramPowerShellPath { get; set; } = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
        public string? TelegramPowerShellArguments { get; set; } = "-ExecutionPolicy Unrestricted -NoProfile";
        public string? TelegramLinuxShellPath { get; set; } = "/bin/bash";
        public string? TelegramLinuxShellArguments { get; set; } = "";
        public string? TelegramPythonPath { get; set; } = "/usr/bin/python";
        public string? TelegramPythonArguments { get; set; } = "";
        public int TelegramResultMaxLength { get; set; } = 12000;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
