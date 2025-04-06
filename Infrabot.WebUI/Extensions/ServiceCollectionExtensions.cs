using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

namespace Infrabot.WebUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrabotControllerServices(this IServiceCollection services)
        {
            // Register services related to the Item API
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IUserService, UserService>();

            // Return the IServiceCollection for method chaining
            return services; 
        }

        public static IServiceCollection AddInfrabotAuthentication(this IServiceCollection services)
        {
            // Register services related to the Item API
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(10);
                options.SlidingExpiration = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
            });
            services.ConfigureApplicationCookie(options => { options.ExpireTimeSpan = TimeSpan.FromHours(10); });

            // Return the IServiceCollection for method chaining
            return services;
        }

        public static IServiceCollection AddInfrabotLogging(this IServiceCollection services)
        {
            // Register services related to the Item API
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(ConfigKeys.EnvironmentVariable) ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

            services.AddSerilog();

            //builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            //builder.Logging.AddConsole();
            //builder.Logging.AddEventLog();

            // Return the IServiceCollection for method chaining
            return services;
        }
    }
}
