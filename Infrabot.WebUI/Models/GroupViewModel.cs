using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Infrabot.WebUI.Models
{
    public class GroupViewModel
    {
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter Name")]
        public string Name { get; set; }

        // Multi-select for telegram users
        [BindProperty]
        [Required(ErrorMessage = "Please select Telegram Users")]
        public List<int> SelectedTelegramUserIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AvailableTelegramUsers { get; set; } = new List<SelectListItem>();
    }
}
