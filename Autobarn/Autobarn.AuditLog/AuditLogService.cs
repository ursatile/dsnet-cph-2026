using Autobarn.Messages;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreAudio;

namespace Autobarn.AuditLog {
	internal class AuditLogService(
		IBus bus, ILogger<AuditLogService> logger
	): IHostedService {
		const string SUBSCRIBER_ID = "autobarn.auditlog";
		private readonly Player player = new();

		public async Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting Autobarn AuditLogService...");
			await bus.PubSub.SubscribeAsync<NewVehicleMessage>(SUBSCRIBER_ID,
				HandleNewVehicleMessage, cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Stopping Autobarn AuditLogService...");
			return Task.CompletedTask;
		}

		private async Task HandleNewVehicleMessage(NewVehicleMessage message) {
			await Task.Delay(TimeSpan.FromSeconds(1));
			try {
				await player.Play("sample.wav");
			} catch(Exception) {
				logger.LogWarning("baaaaaaaa");
			}
			logger.LogInformation("New Vehicle: {message}", message);			
		}
	}
}
