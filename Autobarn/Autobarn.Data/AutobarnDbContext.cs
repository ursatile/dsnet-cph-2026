using Autobarn.Data.Entities;
using Autobarn.Data.Sample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Autobarn.Data;

public class AutobarnDbContext(
	DbContextOptions<AutobarnDbContext> options
) : DbContext(options) {

	public virtual DbSet<Make> Makes { get; set; }
	public virtual DbSet<CarModel> Models { get; set; }
	public virtual DbSet<Vehicle> Vehicles { get; set; }

	public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
		if(Database.IsSqlite()) {
			configurationBuilder.Properties<string>().UseCollation("NOCASE");
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {

		modelBuilder.Entity<OutboxMessage>(entity => {
			if(Database.IsSqlite()) {
				entity.Property(m => m.Id).UseAutoincrement();
			}

			if(Database.IsSqlServer()) {
				entity.Property(m => m.Id).UseIdentityColumn(UInt32.MaxValue + 1L);
			}
		});

		modelBuilder.Entity<Make>(entity => {
			entity.HasKey(e => e.Code);
			entity.Property(e => e.Code).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.Name).HasMaxLength(32).IsUnicode(false);
			entity.HasMany(e => e.Models).WithOne(m => m.Make).HasForeignKey(m => m.MakeCode);
		});

		modelBuilder.Entity<CarModel>(entity => {
			entity.HasKey(e => e.Code);
			entity.Property(e => e.Code).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.MakeCode).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.Name).HasMaxLength(32).IsUnicode(false);
			entity.HasMany(e => e.Vehicles).WithOne(v => v.Model).HasForeignKey(v => v.ModelCode);
		});

		modelBuilder.Entity<Vehicle>(entity => {
			entity.HasKey(e => e.Registration);
			entity.Property(e => e.Registration).HasMaxLength(16).IsUnicode(false);
			entity.Property(e => e.Color).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.ModelCode).HasMaxLength(32).IsUnicode(false);
		});

		modelBuilder.Entity<Make>().HasData(SampleData.CarMakeCsvData);
		modelBuilder.Entity<CarModel>().HasData(SampleData.CarModelCsvData);
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
		optionsBuilder.UseAsyncSeeding(async (dbContext, _, cancellationToken) => {
			var db = (AutobarnDbContext)dbContext;
			if(await db.Vehicles.AnyAsync(cancellationToken)) return;
			var models = await db.Models.ToListAsync(cancellationToken);
			var vehiclesToInsert = SampleData.VehicleCsvData
				.Select(csv => new Vehicle {
					Registration = csv.Registration,
					Model = models.Single(m => m.Code == csv.ModelCode),
					Color = csv.Color,
					Year = csv.Year
				});
			await db.Vehicles.AddRangeAsync(vehiclesToInsert, cancellationToken);
			await db.SaveChangesAsync(cancellationToken);
		});
	}
}
