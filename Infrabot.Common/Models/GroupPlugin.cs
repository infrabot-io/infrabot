using System.ComponentModel.DataAnnotations.Schema;

namespace Infrabot.Common.Models
{
    public class GroupPlugin
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group? Group { get; set; }

        public int PluginId { get; set; }
        [ForeignKey("PluginId")]
        public Plugin? Plugin { get; set; }
    }
}
