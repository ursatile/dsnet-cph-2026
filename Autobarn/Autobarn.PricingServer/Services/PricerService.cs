using Autobarn.PricingEngine;
using Grpc.Core;

namespace Autobarn.PricingServer.Services;

public class PricerService(ILogger<PricerService> logger) : Pricer.PricerBase {
	private Random random = new Random();
	private string ChooseRandomCurrency() {
		var currencies = new[] { "USD", "GBP", "EUR", "JPY", "DKK", "NOK", "SEK", "HUF" };
		return currencies[random.Next(currencies.Length)];
	}

	public override Task<PriceReply> GetPrice(PriceRequest request, ServerCallContext context) {
		logger.LogInformation("Getting price for {request}", request);
		if(request.Make == "FORD") return Task.FromResult(new PriceReply {
			CurrencyCode = "USD",
			Price = 25000
		});
		if(request.Color == "Brown") return Task.FromResult(new PriceReply {
			CurrencyCode = "GBP",
			Price = 50
		});
		return Task.FromResult(new PriceReply {
			CurrencyCode = ChooseRandomCurrency(),
			Price = random.Next(1000, 250000)
		});
	}
}
