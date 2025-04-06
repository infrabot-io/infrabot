using System.ComponentModel.DataAnnotations;

namespace Infrabot.WebUI.Models
{
    public class LoginViewModel
    {
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public bool LoginOrPasswordIncorrect { get; set; } = false;
        public bool LoginDenied { get; set; } = false;
        public bool ADAuthFailed { get; set; } = false;
    }
}
