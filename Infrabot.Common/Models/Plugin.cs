namespace Infrabot.Common.Models
{
    public class Plugin
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string? Name { get; set; }
        public string? PluginType { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation property for group-to-plugin relationships
        public ICollection<GroupPlugin>? GroupPlugins { get; set; }

        // New unified permission assignments.
        public ICollection<PermissionAssignment> PermissionAssignments { get; set; } = new List<PermissionAssignment>();
    }
}
