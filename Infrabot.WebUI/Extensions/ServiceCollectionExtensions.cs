using Infrabot.WebUI.Services;

namespace Infrabot.WebUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrabotControllerServices(this IServiceCollection services)
        {
            // Register services related to the Item API
            services.AddScoped<IApiService, ApiService>();

            // Return the IServiceCollection for method chaining
            return services; 
        }
    }
}
