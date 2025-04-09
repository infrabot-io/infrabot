using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrabot.WorkerService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrabotServices(this IServiceCollection services)
        {
            // Register services related to the Item API
            services.AddHostedService<HealthChecker>();
            services.AddHostedService<HealthDataCleaner>();
            services.AddHostedService<MessageCleaner>();

            // Return the IServiceCollection for method chaining
            return services;
        }

        public static IServiceCollection AddInfrabotLogging(this IServiceCollection services)
        {
            // Register services related to the Item API
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
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
