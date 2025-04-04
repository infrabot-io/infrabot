using Infrabot.Common.Contexts;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

/*****************************************/
/* Add services to the container         */
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<InfrabotContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString(ConfigKeys.DefaultConnection), b=> b.MigrationsAssembly("Infrabot.WebUI")));

/*****************************************/
/* Add authentication mechanism          */
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(10);
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
});
builder.Services.ConfigureApplicationCookie(options => { options.ExpireTimeSpan = TimeSpan.FromMinutes(30); });

/*****************************************/
/* Enable logs to appear in events       */
var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(ConfigKeys.EnvironmentVariable) ?? "Production"}.json", true)
        .Build();

Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

builder.Services.AddSerilog();
//builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
//builder.Logging.AddConsole();
//builder.Logging.AddEventLog();

/*****************************************/
/* Register Services                     */
builder.Services.AddInfrabotControllerServices();

/*****************************************/
/* Build application                     */
var app = builder.Build();

/*****************************************/
/* Configure the HTTP request pipeline   */
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(RoutePaths.Error);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*****************************************/
/*      Ensure database is created       */
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InfrabotContext>();
    dbContext.Database.EnsureCreated();
}

/*****************************************/
/*       Configure error codes           */
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = RoutePaths.Error404;
        await next();
    }
});

/*****************************************/
/* Map services                          */
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: RoutePaths.DefaultRoute);

/*****************************************/
/* Run web application                   */
app.Run();
