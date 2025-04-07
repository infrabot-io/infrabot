using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface ITelegramUsersService
    {
        Task<IEnumerable<TelegramUser>> GetTelegramUsers(int page = 0, int pageSize = 50);
        Task<IEnumerable<TelegramUser>> GetAllTelegramUsers();
        Task<int> GetTelegramUsersCount();
    }

    public class TelegramUsersService : ITelegramUsersService
    {

        private readonly InfrabotContext _context;

        public TelegramUsersService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TelegramUser>> GetTelegramUsers(int page = 0, int pageSize = 50)
        {
            var telegramUsers = await _context.TelegramUsers.OrderBy(s => s.Name).Skip(page * pageSize).Take(pageSize).ToListAsync();
            return telegramUsers;
        }

        public async Task<IEnumerable<TelegramUser>> GetAllTelegramUsers()
        {
            var telegramUsers = await _context.TelegramUsers.ToListAsync();
            return telegramUsers;
        }

        public async Task<int> GetTelegramUsersCount()
        {
            int usersCount = await _context.TelegramUsers.CountAsync();
            return usersCount;
        }
    }
}
