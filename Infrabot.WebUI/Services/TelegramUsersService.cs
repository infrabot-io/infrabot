using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface ITelegramUsersService
    {
        Task<IEnumerable<TelegramUser>> GetTelegramUsers(int page = 0, int pageSize = 50);
        Task<TelegramUser> GetTelegramUserById(int id);
        Task<TelegramUser> GetTelegramUserByTelegramId(int telegramId);
        Task<IEnumerable<TelegramUser>> GetAllTelegramUsers();
        Task<int> GetTelegramUsersCount();
        Task CreateTelegramUser(TelegramUser telegramUser);
        Task DeleteTelegramUser(TelegramUser telegramUser);
        Task UpdateTelegramUser(TelegramUser telegramUser);
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

        public async Task<TelegramUser> GetTelegramUserById(int id)
        {
            var telegramUser = await _context.TelegramUsers.FirstOrDefaultAsync(s => s.Id == id);
            return telegramUser;
        }

        public async Task<TelegramUser> GetTelegramUserByTelegramId(int telegramId)
        {
            var telegramUser = await _context.TelegramUsers.Where(x => x.TelegramId == telegramId).FirstOrDefaultAsync();
            return telegramUser;
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

        public async Task CreateTelegramUser(TelegramUser telegramUser)
        {
            await _context.TelegramUsers.AddAsync(telegramUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTelegramUser(TelegramUser telegramUser)
        {
            _context.TelegramUsers.Remove(telegramUser);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTelegramUser(TelegramUser telegramUser)
        {
            _context.TelegramUsers.Update(telegramUser);
            await _context.SaveChangesAsync();
        }
    }
}
