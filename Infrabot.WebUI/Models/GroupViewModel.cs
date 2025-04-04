using Microsoft.AspNetCore.Mvc.Rendering;

namespace Infrabot.WebUI.Models
{
    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Multi-select for telegram users
        public List<int> SelectedTelegramUserIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailableTelegramUsers { get; set; } = new List<SelectListItem>();
    }
}
