using Autobarn.Data;
using Autobarn.Website.Api;
using Autobarn.Website.Services;
using EasyNetQ;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
	tracing.AddEntityFrameworkCoreInstrumentation());

var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger<Program>();
#if SQLITE
logger.LogInformation("Using in-memory database");
SqliteConnection sqliteConnection = new($"Data Source=:memory:");
sqliteConnection.Open();
builder.Services.AddDbContext<AutobarnDbContext>(options => options.UseSqlite(sqliteConnection));
#else
logger.LogInformation("Using SQL Server database");
var sqlConnectionString = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
builder.Services.AddDbContext<AutobarnDbContext>(options => options.UseSqlServer(sqlConnectionString));
#endif

builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();

var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
builder.Services.AddEasyNetQ(rabbitmq);

builder.Services.AddSingleton<OutboxHostedService>();
builder.Services.AddHostedService(services
	=> services.GetRequiredService<OutboxHostedService>());

var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

using var scope = app.Services.CreateScope();
await using var db = scope.ServiceProvider.GetRequiredService<AutobarnDbContext>();
await db.Database.EnsureCreatedAsync();

if(app.Environment.IsProduction()) app.UseHttpsRedirection();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapAutobarnApi();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
