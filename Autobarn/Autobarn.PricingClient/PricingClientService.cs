using Autobarn.Messages;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreAudio;

namespace Autobarn.PricingClient {
	internal class PricingClientService(
		IBus bus, ILogger<PricingClientService> logger
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
			await player.Play("sample.wav");
			logger.LogInformation("New Vehicle: {message}", message);
		}
	}
}
