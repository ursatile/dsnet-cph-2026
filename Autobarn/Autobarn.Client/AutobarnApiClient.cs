using System.Net.Http.Json;

namespace Autobarn.Client;

public class AutobarnApiClient {

	private readonly HttpClient http;
	private readonly Lazy<Task<string[]>> modelCodesCache;

	public AutobarnApiClient(HttpClient http) {
		this.http = http;
		modelCodesCache = new Lazy<Task<string[]>>(FetchModelCodesAsync);
	}

	private async Task<string[]> FetchModelCodesAsync() {
		var models = await http.GetFromJsonAsync<List<ModelDto>>("/api/carmodels")
			?? throw new InvalidOperationException("GET /api/carmodels returned no data.");
		return models.Select(m => m.Code).ToArray();
	}

	public Task<string[]> ListModelCodesAsync() => modelCodesCache.Value;

	public async Task<VehicleDto> CreateRandomVehicleAsync() {
		var modelCodes = await ListModelCodesAsync();
		var vehicle = new VehicleDto {
			Registration = RandomRegistration(),
			ModelCode = modelCodes[Random.Shared.Next(modelCodes.Length)],
			Color = NamedColors.All[Random.Shared.Next(NamedColors.All.Length)],
			Year = Random.Shared.Next(1960, 2026)
		};
		var response = await http.PutAsJsonAsync($"/api/vehicles/{vehicle.Registration}", vehicle);
		response.EnsureSuccessStatusCode();
		return vehicle;
	}

	private const string RegistrationChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	private static string RandomRegistration() {
		Span<char> chars = stackalloc char[8];
		for (var i = 0; i < chars.Length; i++) {
			chars[i] = RegistrationChars[Random.Shared.Next(RegistrationChars.Length)];
		}
		return new string(chars);
	}

	private sealed class ModelDto {
		public string Code { get; set; } = "";
	}
}