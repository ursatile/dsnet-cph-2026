using Autobarn.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger<Program>();
logger.LogInformation("Using in-memory database");
SqliteConnection sqliteConnection = new($"Data Source=:memory:");
sqliteConnection.Open();
builder.Services.AddDbContext<AutobarnDbContext>(options => options.UseSqlite(sqliteConnection));
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

using var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<AutobarnDbContext>();
await db.Database.EnsureCreatedAsync();

if (app.Environment.IsProduction()) app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
