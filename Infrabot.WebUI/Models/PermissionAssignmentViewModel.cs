using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Infrabot.WebUI.Models
{
    public class PermissionAssignmentViewModel
    {
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide name")]
        public string Name { get; set; }

        // Multi-select for plugins.
        [BindProperty]
        [Required(ErrorMessage = "Please select Plugins")]
        public List<int> SelectedPluginIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailablePlugins { get; set; } = new List<SelectListItem>();

        // Multi-select for Telegram users.
        [BindProperty]
        [Required(ErrorMessage = "Please select Telegram Users")]
        public List<int> SelectedTelegramUserIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailableTelegramUsers { get; set; } = new List<SelectListItem>();

        // Multi-select for Groups.
        [BindProperty]
        [Required(ErrorMessage = "Please select Groups")]
        public List<int> SelectedGroupIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();
    }
}
