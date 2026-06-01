using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Messages;
using Autobarn.Website.Models;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Media;

namespace Autobarn.Website.Api;

public static class EndpointRouteBuilderExtensions {
	public static IEndpointRouteBuilder MapAutobarnApi(
		this IEndpointRouteBuilder app,
		string path = "/api"
	) {
		app.MapGet("/api/hello", () => "Hello world!");
		app.MapGet("/api/hello/{name}", (string name) => $"Hello {name}!");
		app.MapGet("/api/makes", (AutobarnDbContext db) => db.Makes.ToList());
		app.MapGet("/api/carmodels", (AutobarnDbContext db) => db.Models.ToList());

		app.MapPut("/api/vehicles/{registration}", async (AutobarnDbContext db,
			string registration,
			VehicleDto dto) => {
				if(registration != dto.Registration) return Results.BadRequest();
				var vehicle = await db.Vehicles.FindAsync(dto.Registration);
				if(vehicle == null) {
					var newVehicle = new Vehicle {
						Color = dto.Color,
						Registration = dto.Registration,
						Year = dto.Year,
						ModelCode = dto.ModelCode
					};

					await db.Vehicles.AddAsync(newVehicle);
					await db.SaveChangesAsync();
					return Results.Created($"/api/vehicles/{newVehicle.Registration}", newVehicle);
				}

				vehicle.Color = dto.Color;
				vehicle.Year = dto.Year;
				vehicle.ModelCode = dto.ModelCode;
				await db.SaveChangesAsync();
				return Results.NoContent();
			});

		app.MapPost("/api/models/{modelCode}/vehicles",
				async (
					AutobarnDbContext db,
					[Description("The vehicle to be added to the system")]
			VehicleDto dto,
					string modelCode,
					ILogger<Program> logger,
					IBus bus
				) => {
					var existing = db.Vehicles.Find(dto.Registration);
					if(existing != null) {
						logger.LogWarning("We already have a vehicle with registration {reg} on our system.", dto.Registration);
						return Results.Conflict($"We already have a vehicle with registration {dto.Registration} on our system.");
					}
					var carModel = db.Models
						.Include(m => m.Make)
						.FirstOrDefault(m => m.Code == modelCode);

					if(carModel == null) {
						return Results.NotFound($"There is no model code matching {modelCode} in our database");
					}
					if(dto.Year > DateTimeOffset.UtcNow.Year) return Results.BadRequest("Time travel not permitted!");

					var vehicle = new Vehicle {
						Color = dto.Color,
						Registration = dto.Registration,
						Year = dto.Year,
						Model = carModel
					};
					await db.Vehicles.AddAsync(vehicle);
					await db.SaveChangesAsync();

					// Play a WAV file when a new vehicle is added
					try {
						var soundPlayer = new SoundPlayer("boing.wav");
						soundPlayer.Play(); // Play asynchronously
					} catch(Exception ex) {
						logger.LogWarning(ex, "Failed to play sound notification for new vehicle");
					}

					var message = new NewVehicleMessage() {
						Color = dto.Color,
						Make = carModel.Make.Name,
						Model = carModel.Name,
						Registration = dto.Registration!,
						CreatedAt = DateTimeOffset.UtcNow,
						Year = dto.Year
					};
					await bus.PubSub.PublishAsync(message);
					logger.LogInformation("Created new vehicle: {reg} ({make}, {model}, {color}, {year})",
						dto.Registration, carModel.Make.Name, carModel.Name, dto.Color, dto.Year);
					return Results.Created($"/api/vehicles/{vehicle.Registration}", vehicle);
				})
				.WithSummary("Add a new vehicle to Autobarn's platform")
				.WithDescription("List a new vehicle for sale on the platform. Returns 409 if the registration is already assigned, etc, etc.")
				.Produces(StatusCodes.Status201Created)
				.Produces(StatusCodes.Status409Conflict)
				.Produces(StatusCodes.Status404NotFound)
				.Produces(StatusCodes.Status400BadRequest);
		return app;
	}
}
