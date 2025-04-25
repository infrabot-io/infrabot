namespace Infrabot.Common.Domain
{
    public class HealthCheckItem
    {
        public HealthCheckItem() { }
        public int RamUsage { get; set; }
        public int CpuUsage { get; set; }
    }
}
