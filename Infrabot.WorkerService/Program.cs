using Infrabot.Common.Contexts;
using Infrabot.WorkerService.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

/*****************************************/
/* Add database connection               */
builder.Services.AddDbContext<InfrabotContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

/*****************************************/
/* Add services                          */
builder.Services.AddWindowsService(options => { options.ServiceName = "Infrabot Worker Service"; });
builder.Services.AddInfrabotLogging();
builder.Services.AddInfrabotServices();

var host = builder.Build();
host.Run();
