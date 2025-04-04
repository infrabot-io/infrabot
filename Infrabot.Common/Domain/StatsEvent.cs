using Infrabot.Common.Enums;

namespace Infrabot.Common.Domain
{
    public class StatsEvent
    {
        public StatsEvent() { }
        public DateTime EventDate { get; set; }
        public EventLogType EventType { get; set; }
        public string Description { get; set; }
    }
}
