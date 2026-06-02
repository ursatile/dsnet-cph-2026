using Autobarn.Messages;
using EasyNetQ;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreAudio;
using System.Text.Json;

namespace Autobarn.Notifier {
	internal class NotifierService(
		IBus bus, ILogger<NotifierService> logger,
		HubConnection hub
	): IHostedService {
		const string SUBSCRIBER_ID = "autobarn.Notifier";
		private readonly Player player = new();

		public async Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting Autobarn NotifierService...");
			await bus.PubSub.SubscribeAsync<NewVehiclePriceMessage>(SUBSCRIBER_ID,
				HandleNewVehiclePriceMessage, cancellationToken);
			await hub.StartAsync(cancellationToken);
			logger.LogInformation("Started Autobarn NotifierService!");
		}

		public async Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Stopping Autobarn NotifierService...");
			await hub.StopAsync(cancellationToken);
			logger.LogInformation("Stopped Autobarn NotifierService!");
		}

		private async Task HandleNewVehiclePriceMessage(NewVehiclePriceMessage message) {
			await Task.Delay(TimeSpan.FromSeconds(1));
			var json = JsonSerializer.Serialize(message);
			await SendWithReconnect(json);
			try {
				await player.Play("sample.wav");
			} catch(Exception) {
				logger.LogWarning("baaaaaaaa");
			}			
		}

		private async Task SendWithReconnect(string json, int maxRetries = 5) {
			for(var attempt = 0; ; attempt++) {
				try {
					if(hub.State == HubConnectionState.Disconnected) await hub.StartAsync();
					await hub.SendAsync("NotifyWebsiteUsers", "autobarn.notifier", json);
					return;
				} catch(Exception ex) when(attempt < maxRetries) {
					var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
					logger.LogWarning("Send failed (attempt {attempt}): {message}. Retrying in {delay}s...", attempt + 1, ex.Message, delay.TotalSeconds);
					await Task.Delay(delay);
				}
			}
		}
	}
}
