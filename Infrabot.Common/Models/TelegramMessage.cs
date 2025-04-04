namespace Infrabot.Common.Models
{
    public class TelegramMessage
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public long? TelegramUserId { get; set; }
        public string? TelegramUserUsername { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
