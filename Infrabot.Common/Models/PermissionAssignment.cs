namespace Infrabot.Common.Models
{
    public class PermissionAssignment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Many-to-many: one permission assignment can relate to many plugins.
        public ICollection<Plugin> Plugins { get; set; } = new List<Plugin>();

        // Many-to-many: assigned Telegram users.
        public ICollection<TelegramUser> TelegramUsers { get; set; } = new List<TelegramUser>();

        // Many-to-many: assigned Groups.
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
