using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrabot.Common.Models
{
    public class Group
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation property for group-to-user relationships
        public List<UserGroup>? UserGroups { get; set; }

        // Navigation property for group-to-plugin relationships
        public ICollection<GroupPlugin>? GroupPlugins { get; set; }
    }
}
