#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Autobarn.Messages;
using System.Text.Json;

namespace Autobarn.Data.Entities;

public class Vehicle {
	public string? Registration { get; set; }
	public string? Color { get; set; }
	public int Year { get; set; }
	public CarModel Model { get; set; }
	public string ModelCode { get; set; }
}

public class OutboxMessage {
	public ulong Id { get; set; }
	public string MessageType { get; set; } = "";
	public string MessageJson { get; set; } = default!;
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? SentAt { get; set; }
	public int FailureCount { get; set; } = 0;
	public string? FailureMessage { get; set; } = null;

	public OutboxMessage() { }

	public OutboxMessage(NewVehicleMessage newVehicleMessage) {
		this.MessageType = nameof(NewVehicleMessage);
		this.MessageJson = JsonSerializer.Serialize(newVehicleMessage);
		this.CreatedAt = DateTimeOffset.UtcNow;
	}
}
