using System.ComponentModel.DataAnnotations.Schema;

namespace Infrabot.Common.Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group? Group { get; set; }

        public int TelegramUserId { get; set; }
        [ForeignKey("TelegramUserId")]
        public TelegramUser? TelegramUser { get; set; }
    }
}
