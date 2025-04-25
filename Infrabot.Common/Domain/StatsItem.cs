namespace Infrabot.Common.Domain
{
    public class StatsItem
    {
        public StatsItem()
        {
            Plugins = 0;
            Users = 0;
            TelegramUsers = 0;
            TelegramMessages = 0;
            StatsEvents = new List<StatsEvent> { };
        }
        public int Plugins { get; set; }
        public int Users { get; set; }
        public int TelegramUsers { get; set; }
        public int TelegramMessages { get; set; }
        public List<StatsEvent> StatsEvents { get; set; }
    }
}
