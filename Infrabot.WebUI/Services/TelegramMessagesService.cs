using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface ITelegramMessagesService
    {
        Task<IEnumerable<TelegramMessage>> GetTelegramMessages(int page = 0, int pageSize = 50);
        Task<IEnumerable<TelegramMessage>> GetAllTelegramMessages();
        Task<int> GetTelegramMessagesCount();
    }

    public class TelegramMessagesService : ITelegramMessagesService
    {
        private readonly InfrabotContext _context;

        public TelegramMessagesService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TelegramMessage>> GetTelegramMessages(int page = 0, int pageSize = 50)
        {
            var telegramMessages = await _context.TelegramMessages.OrderByDescending(s => s.CreatedDate).Skip(page * pageSize).Take(pageSize).ToListAsync();
            return telegramMessages;
        }

        public async Task<IEnumerable<TelegramMessage>> GetAllTelegramMessages()
        {
            var telegramMessages = await _context.TelegramMessages.ToListAsync();
            return telegramMessages;
        }

        public async Task<int> GetTelegramMessagesCount()
        {
            int telegramMessagesCount = await _context.TelegramMessages.CountAsync();
            return telegramMessagesCount;
        }
    }
}
