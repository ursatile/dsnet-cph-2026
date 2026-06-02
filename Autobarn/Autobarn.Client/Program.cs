using Autobarn.Client;

var baseAddress = args.Length > 0 ? args[0] : "https://autobarn.dev";

using var http = new HttpClient();
http.BaseAddress = new Uri(baseAddress);
var client = new AutobarnApiClient(http);

Console.WriteLine($"Autobarn API client — connecting to {baseAddress}");
Console.Write("Loading model codes... ");
var modelCodes = await client.ListModelCodesAsync();
Console.WriteLine($"loaded {modelCodes.Length} codes.");

Console.WriteLine("Press any key to create a random vehicle, or Ctrl+C to quit.");

while (true) {
	Console.ReadKey(intercept: true);
	try {
		var vehicle = await client.CreateRandomVehicleAsync();
		Console.WriteLine($"Created: {vehicle.Registration}  {vehicle.Year}  {vehicle.ModelCode}  {vehicle.Color}");
	}
	catch (HttpRequestException ex) {
		Console.WriteLine($"Request failed: {ex.Message}");
	}
}