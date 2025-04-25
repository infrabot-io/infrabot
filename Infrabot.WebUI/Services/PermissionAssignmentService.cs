using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IPermissionAssignmentService
    {
        Task<IEnumerable<PermissionAssignment>> GetPermissionAssignments(int page = 0, int pageSize = 50);
        Task<PermissionAssignment> GetPermissionAssignmentById(int id);
        Task<IEnumerable<PermissionAssignment>> GetAllPermissionAssignments();
        Task<int> GetPermissionAssignmentsCount();
        Task UpdatePermissionAssignment(PermissionAssignment permissionAssignment);
        Task CreatePermissionAssignment(PermissionAssignment permissionAssignment);
        Task DeletePermissionAssignment(PermissionAssignment permissionAssignment);
    }

    public class PermissionAssignmentService : IPermissionAssignmentService
    {
        private readonly InfrabotContext _context;

        public PermissionAssignmentService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PermissionAssignment>> GetPermissionAssignments(int page = 0, int pageSize = 50)
        {
            var permissionAssignments = await _context.PermissionAssignments.OrderBy(s => s.Name).Include(pa => pa.Plugins).Include(pa => pa.TelegramUsers).Include(pa => pa.Groups).Skip(page * pageSize).Take(pageSize).AsSplitQuery().ToListAsync();
            return permissionAssignments;
        }

        public async Task<PermissionAssignment> GetPermissionAssignmentById(int id)
        {
            var permissionAssignment = await _context.PermissionAssignments.Include(pa => pa.Plugins).Include(pa => pa.TelegramUsers).Include(pa => pa.Groups).FirstOrDefaultAsync(pa => pa.Id == id);
            return permissionAssignment;
        }

        public async Task<IEnumerable<PermissionAssignment>> GetAllPermissionAssignments()
        {
            var permissionAssignments = await _context.PermissionAssignments.ToListAsync();
            return permissionAssignments;
        }

        public async Task<int> GetPermissionAssignmentsCount()
        {
            int permissionAssignmentsCount = await _context.PermissionAssignments.CountAsync();
            return permissionAssignmentsCount;
        }

        public async Task UpdatePermissionAssignment(PermissionAssignment permissionAssignment)
        {
            _context.PermissionAssignments.Update(permissionAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task CreatePermissionAssignment(PermissionAssignment permissionAssignment)
        {
            await _context.PermissionAssignments.AddAsync(permissionAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePermissionAssignment(PermissionAssignment permissionAssignment)
        {
            _context.PermissionAssignments.Remove(permissionAssignment);
            await _context.SaveChangesAsync();
        }
    }
}
