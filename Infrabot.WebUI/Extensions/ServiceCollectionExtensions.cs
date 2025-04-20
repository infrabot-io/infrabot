using Infrabot.Common.Contexts;
using Infrabot.Common.Models;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Services;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Infrabot.WebUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrabotControllerServices(this IServiceCollection services)
        {
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IAuditLogsService, AuditLogsService>();
            services.AddScoped<IGroupsService, GroupsService>();
            services.AddScoped<ITelegramUsersService, TelegramUsersService>();
            services.AddScoped<IUserGroupsService, UserGroupsService>();
            services.AddScoped<ITelegramMessagesService, TelegramMessagesService>();
            services.AddScoped<IPluginsService, PluginsService>();
            services.AddScoped<IPermissionAssignmentService, PermissionAssignmentService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();

            return services; 
        }

        public static IServiceCollection AddInfrabotAuthentication(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(ConfigKeys.EnvironmentVariable) ?? "Production"}.json", true)
                .Build();

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<InfrabotContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/account/accessdenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(10);
                options.Cookie.Name = "InfrabotCookie";
                options.LoginPath = "/account/login";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.SlidingExpiration = true;
            });

            services.Configure<IdentityOptions>(options =>
            {
                configuration.GetSection(nameof(IdentityOptions)).Bind(options);
            });
            
            return services;
        }

        public static IServiceCollection AddInfrabotLogging(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(ConfigKeys.EnvironmentVariable) ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

            services.AddSerilog();

            return services;
        }
    }
}
