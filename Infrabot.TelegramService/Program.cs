using Infrabot.Common.Contexts;
using Infrabot.TelegramService.Core;
using Infrabot.TelegramService.Managers;
using Infrabot.TelegramService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

/*****************************************/
/* Add service definition for Windows    */
builder.Services.AddWindowsService(options => { options.ServiceName = "Infrabot Telegram Bot Service"; });

/*****************************************/
/* Add database connection               */
builder.Services.AddDbContext<InfrabotContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

/*****************************************/
/* Enable logs to appear in events       */
var configuration = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json")
       .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
       .Build();

builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

/*****************************************/
/* Enable main Worker service            */
//builder.Services.AddHostedService<InfrabotWorker>();
//builder.Services.AddHostedService<PluginManager>();
builder.Services.AddSingleton<IEmergencyStateManager, EmergencyStateManager>();
builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<IPluginRegistry>(sp => (PluginManager)sp.GetRequiredService<PluginManager>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<PluginManager>());
builder.Services.AddHostedService<TelegramService>();

var host = builder.Build();
host.Run();
