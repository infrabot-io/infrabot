using Infrabot.Common.Enums;

namespace Infrabot.Common.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string? IPAddress { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public AuditLogAction LogAction { get; set; }
        public AuditLogItem LogItem { get; set; }
        public AuditLogResult LogResult { get; set; }
        public AuditLogSeverity LogSeverity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
