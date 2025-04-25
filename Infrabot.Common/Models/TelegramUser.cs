namespace Infrabot.Common.Models
{
    public class TelegramUser
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public int TelegramId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation property for user-to-group relationships
        public List<UserGroup>? UserGroups { get; set; }
    }
}
