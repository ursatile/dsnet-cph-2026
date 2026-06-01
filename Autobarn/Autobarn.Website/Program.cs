using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
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

app.MapGet("/api/hello", () => "Hello world!");
app.MapGet("/api/hello/{name}", (string name) => $"Hello {name}!");
app.MapGet("/api/makes", (AutobarnDbContext db) => db.Makes.ToList());
app.MapGet("/api/vehicles/{registration}",
	(string registration, AutobarnDbContext db) =>
		db.Vehicles.Find(registration));

//TODO: implement this:
app.MapPut("/api/vehicles/{registration}", async (AutobarnDbContext db, VehicleDto dto) => {
	// what goes here?
});

app.MapPost("/api/vehicles", async (AutobarnDbContext db, VehicleDto dto) => {
	var carModel = db.Models.FirstOrDefault(m => m.Code == dto.ModelCode);
	var vehicle = new Vehicle {
		Color = dto.Color,
		Registration = dto.Registration,
		Year = dto.Year,
		Model = carModel
	};
	await db.Vehicles.AddAsync(vehicle);
	await db.SaveChangesAsync();
	return Results.Created($"/api/vehicles/{vehicle.Registration}", vehicle);
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
