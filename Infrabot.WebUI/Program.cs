using Infrabot.Common.Contexts;
using Infrabot.WebUI.Constants;
using Infrabot.WebUI.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/*****************************************/
/* Add services                          */
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<InfrabotContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString(ConfigKeys.DefaultConnection), b=> b.MigrationsAssembly("Infrabot.WebUI")));
builder.Services.AddInfrabotAuthentication();
builder.Services.AddInfrabotLogging();
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
