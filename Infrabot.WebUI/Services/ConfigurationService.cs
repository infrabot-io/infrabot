using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.WebUI.Services
{
    public interface IConfigurationService
    {
        Task<Configuration> GetConfiguration();
        Task UpdateConfiguration(Configuration configuration);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly InfrabotContext _context;

        public ConfigurationService(InfrabotContext context)
        {
            _context = context;
        }

        public async Task<Configuration> GetConfiguration()
        {
            var configuration = await _context.Configurations.FirstOrDefaultAsync(s => s.Id == 1);
            return configuration;
        }

        public async Task UpdateConfiguration(Configuration configuration)
        {
            _context.Configurations.Update(configuration);
            await _context.SaveChangesAsync();
        }
    }
}
