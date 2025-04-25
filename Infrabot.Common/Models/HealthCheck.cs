namespace Infrabot.Common.Models
{
    public class HealthCheck
    {
        public int Id { get; set; }
        public string? Data { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
