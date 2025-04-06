using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsers(int page = 0, int pageSize = 50);
        Task<int> GetUsersCount();
    }

    public class UserService : IUserService
    {

        private readonly InfrabotContext _context;

        public UserService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsers(int page = 0, int pageSize = 50)
        {
            var users = await _context.Users.OrderBy(s => s.UserName).Skip(page * pageSize).Take(pageSize).ToListAsync();
            return users;
        }

        public async Task<int> GetUsersCount()
        {
            int usersCount = await _context.Users.CountAsync();
            return usersCount;
        }
    }
}
