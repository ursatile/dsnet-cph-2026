namespace Autobarn.Data.Sample;

public static class SampleData
{

	public record CarModelCsvRecord(string Code, string MakeCode, string Name);

	public record CarMakeCsvRecord(string Code, string Name);

	public record VehicleCsvRecord(string Registration, string ModelCode, string Color, int Year);

	public static IEnumerable<CarModelCsvRecord> CarModelCsvData
		=> EmbeddedResource.ReadAllLines("models.csv", typeof(SampleData).Assembly)
			.Select(line => line.Split(","))
			.Where(tokens => tokens.Length == 3)
			.Select(tokens => new CarModelCsvRecord(tokens[0], tokens[1], tokens[2]));

	public static IEnumerable<CarMakeCsvRecord> CarMakeCsvData
		=> EmbeddedResource.ReadAllLines("makes.csv", typeof(SampleData).Assembly)
			.Select(line => line.Split(","))
			.Where(tokens => tokens.Length == 2)
			.Select(tokens => new CarMakeCsvRecord(tokens[0], tokens[1]));

	public static IEnumerable<VehicleCsvRecord> VehicleCsvData
		=> EmbeddedResource.ReadAllLines("vehicles.csv", typeof(SampleData).Assembly)
			.Select(line => line.Split(","))
			.Where(tokens => tokens.Length == 4)
			.Select(tokens => new VehicleCsvRecord(tokens[0], tokens[1], tokens[2], Int32.Parse(tokens[3])));
}
