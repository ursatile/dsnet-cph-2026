using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Messages;
using Autobarn.Website.Models;
using Autobarn.Website.Services;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Media;

namespace Autobarn.Website.Api;

public static class EndpointRouteBuilderExtensions {

	private static async Task CreateVehicle(AutobarnDbContext db, Vehicle vehicle) {
		var message = new NewVehicleMessage() {
			Color = vehicle.Color,
			Make = vehicle.Model.Make.Name,
			Model = vehicle.Model.Name,
			Registration = vehicle.Registration!,
			CreatedAt = DateTimeOffset.UtcNow,
			Year = vehicle.Year
		};

		// Actually list the vehicle for sale.
		await db.Vehicles.AddAsync(vehicle);
		await db.OutboxMessages.AddAsync(new OutboxMessage(message));
		await db.SaveChangesAsync();
	}
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
			VehicleDto dto,
			ILogger<Program> logger,
			OutboxHostedService outbox) => {
				if(registration != dto.Registration) return Results.BadRequest();

				var carModel = db.Models
				.Include(m => m.Make)
				.FirstOrDefault(m => m.Code == dto.ModelCode);
				if(carModel == null) return Results.BadRequest();

				var vehicle = await db.Vehicles.FindAsync(dto.Registration);
				if(vehicle == null) {
					var newVehicle = new Vehicle {
						Color = dto.Color,
						Registration = dto.Registration,
						Year = dto.Year,
						Model = carModel
					};
					await CreateVehicle(db, newVehicle);
					outbox.WakeUpAndDoStuff();
					logger.LogInformation("PUT: Created new vehicle: {vehicle}", vehicle);
					return Results.Created($"/api/vehicles/{newVehicle.Registration}", newVehicle);
				}

				vehicle.Color = dto.Color;
				vehicle.Year = dto.Year;
				vehicle.Model = carModel;
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
					OutboxHostedService outbox
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


					// POWER OUTAGE HERE
					await CreateVehicle(db, vehicle);

					outbox.WakeUpAndDoStuff();

					logger.LogInformation("POST: Created new vehicle: {reg} ({make}, {model}, {color}, {year})",
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
