using SoftFluent.ComponentModel.DataAnnotations;

namespace Infrabot.Common.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        [Encrypted]
        public string? Password { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public bool IsADIntegrated { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public DateTime LastLoginDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
