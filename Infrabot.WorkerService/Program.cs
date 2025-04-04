using Infrabot.Common.Contexts;
using Infrabot.WorkerService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

var builder = Host.CreateApplicationBuilder(args);

/*****************************************/
/* Add service definition for Windows    */
builder.Services.AddWindowsService(options => { options.ServiceName = "Infrabot Worker Service"; });

/*****************************************/
/* Add database connection               */
builder.Services.AddDbContext<InfrabotContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

/*****************************************/
/* Enable logs to appear in events       */
LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddEventLog();

/*****************************************/
/* Enable main Worker service            */
builder.Services.AddHostedService<HealthChecker>();
builder.Services.AddHostedService<HealthDataCleaner>();
builder.Services.AddHostedService<MessageCleaner>();

var host = builder.Build();
host.Run();
