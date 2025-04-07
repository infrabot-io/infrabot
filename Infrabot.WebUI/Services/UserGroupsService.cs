using Infrabot.Common.Contexts;
using Infrabot.Common.Models;

namespace Infrabot.WebUI.Services
{
    public interface IUserGroupsService
    {
        Task AddRangeUserGroups(List<UserGroup> userGroups);
    }
    public class UserGroupsService : IUserGroupsService
    {
        private readonly InfrabotContext _context;

        public UserGroupsService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task AddRangeUserGroups(List<UserGroup> userGroups)
        {
            await _context.UserGroups.AddRangeAsync(userGroups);
            await _context.SaveChangesAsync();
        }
    }
}
