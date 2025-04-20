using Microsoft.AspNetCore.Identity;

namespace Infrabot.Common.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public DateTime LastLoginDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
