using System.ComponentModel.DataAnnotations;

namespace Infrabot.WebUI.Models
{
    public class UserViewModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsADIntegrated { get; set; } = false;
        public bool Enabled { get; set; } = true;
    }
}
