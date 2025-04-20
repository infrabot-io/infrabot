using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IAuditLogsService
    {
        Task<IEnumerable<AuditLog>> GetAuditLogs(int page = 0, int pageSize = 50);
        Task<IEnumerable<AuditLog>> GetAllAuditLogs();
        Task<int> GetAuditLogsCount();
        Task AddAuditLog(AuditLog auditLog);
    }

    public class AuditLogsService : IAuditLogsService
    {
        private readonly InfrabotContext _context;

        public AuditLogsService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogs(int page = 0, int pageSize = 50)
        {
            var auditLogs = await _context.AuditLogs.OrderByDescending(s => s.CreatedDate).Skip(page * pageSize).Take(pageSize).ToListAsync();
            return auditLogs;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogs()
        {
            var auditLogs = await _context.AuditLogs.ToListAsync();
            return auditLogs;
        }

        public async Task<int> GetAuditLogsCount()
        {
            int auditLogsCount = await _context.AuditLogs.CountAsync();
            return auditLogsCount;
        }

        public async Task AddAuditLog(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            var result = await _context.SaveChangesAsync();
        }
    }
}
