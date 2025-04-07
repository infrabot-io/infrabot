using Infrabot.Common.Contexts;
using Infrabot.Common.Models;

namespace Infrabot.WebUI.Services
{
    public interface IAuditLogService
    {
        Task AddAuditLog(AuditLog auditLog);
    }

    public class AuditLogService: IAuditLogService
    {
        private readonly InfrabotContext _context;

        public AuditLogService(InfrabotContext context)
        {
            _context = context;
        }
        public async Task AddAuditLog(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            var result = await _context.SaveChangesAsync();
        }
    }
}
