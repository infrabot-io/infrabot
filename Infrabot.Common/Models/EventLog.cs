using Infrabot.Common.Enums;

namespace Infrabot.Common.Models
{
    public class EventLog
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public EventLogType EventType { get; set; } = EventLogType.None;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
