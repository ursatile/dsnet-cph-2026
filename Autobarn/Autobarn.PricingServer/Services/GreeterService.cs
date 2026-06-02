using Autobarn.PricingEngine;
using Grpc.Core;

namespace Autobarn.PricingServer.Services;

public class GreeterService(ILogger<GreeterService> logger) : Pricer.PricerBase {
	public override Task<PriceReply> GetPrice(PriceRequest request, ServerCallContext context) {
		logger.LogInformation("Getting price for {request}", request);
		return Task.FromResult(new PriceReply {
			CurrencyCode = "DKK",
			Price = 123456
		});
	}
}
