using System.ComponentModel.DataAnnotations;

namespace Infrabot.WebUI.Models
{
    public class ChangePasswordViewModel
    {

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPasswordRepeat { get; set; }
        public bool NewPasswordNotEqualToRepeat { get; set; } = false;
    }
}
