using Autobarn.Messages;
using Autobarn.PricingEngine;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreAudio;

namespace Autobarn.PricingClient {
	internal class PricingClientService(
		IBus bus,
		Pricer.PricerClient pricer,
		ILogger<PricingClientService> logger
	): IHostedService {
		const string SUBSCRIBER_ID = "autobarn.PricingClient";
		private readonly Player player = new();

		public async Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting Autobarn PricingClientService...");
			await bus.PubSub.SubscribeAsync<NewVehicleMessage>(SUBSCRIBER_ID,
				HandleNewVehicleMessage, cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Stopping Autobarn PricingClientService...");
			return Task.CompletedTask;
		}

		private async Task HandleNewVehicleMessage(NewVehicleMessage message) {
			var priceRequest = new PriceRequest {
				Color = message.Color,
				Make = message.Make,
				Model = message.Model,
				Year = message.Year
			};
			var priceReply = await pricer.GetPriceAsync(priceRequest);
			logger.LogInformation("Got price: {price} {currency}", priceReply.Price, priceReply.CurrencyCode);
			await player.Play("sample.wav");
		}
	}
}
