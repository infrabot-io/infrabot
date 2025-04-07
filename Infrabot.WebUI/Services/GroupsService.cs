using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Infrabot.WebUI.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IGroupsService
    {

        Task<IEnumerable<Group>> GetGroups(int page = 0, int pageSize = 50);
        Task<IEnumerable<Group>> GetAllGroups();
        Task<Group> GetGroupById(int id);
        Task<Group> GetGroupByName(string name);
        Task CreateGroup(Group group);
        Task<int> GetGroupsCount();
        Task DeleteGroup(Group group);
        Task UpdateGroup(Group group);
    }

    public class GroupsService: IGroupsService
    {
        private readonly InfrabotContext _context;

        public GroupsService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetGroups(int page = 0, int pageSize = 50)
        {
            var groups = await _context.Groups.OrderBy(s => s.Name).Skip(page * pageSize).Take(pageSize).ToListAsync();
            return groups;
        }

        public async Task<IEnumerable<Group>> GetAllGroups()
        {
            var groups = await _context.Groups.ToListAsync();
            return groups;
        }

        public async Task<Group> GetGroupById(int id)
        {
            var group = await _context.Groups.Include(x => x.UserGroups).FirstOrDefaultAsync(s => s.Id == id);
            return group;
        }

        public async Task<Group> GetGroupByName(string name)
        {
            var group = await _context.Groups.Include(x => x.UserGroups).FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
            return group;
        }

        public async Task CreateGroup(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetGroupsCount()
        {
            int groupsCount = await _context.Groups.CountAsync();
            return groupsCount;
        }

        public async Task DeleteGroup(Group group)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGroup(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }
    }
}
