using Microsoft.AspNetCore.Mvc.Rendering;

namespace Infrabot.WebUI.Models
{
    public class PermissionAssignmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Multi-select for plugins.
        public List<int> SelectedPluginIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailablePlugins { get; set; } = new List<SelectListItem>();

        // Multi-select for Telegram users.
        public List<int> SelectedTelegramUserIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailableTelegramUsers { get; set; } = new List<SelectListItem>();

        // Multi-select for Groups.
        public List<int> SelectedGroupIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();
    }
}
