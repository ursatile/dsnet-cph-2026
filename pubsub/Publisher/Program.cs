using EasyNetQ;
using Messages;
using Microsoft.Extensions.DependencyInjection;

const string AMQP = "amqps://yqtziupn:0TYNVJ3F3JTBd6BWSAh_3jIAYW95AfDJ@sleepy-charcoal-alpaca.rmq7.cloudamqp.com/yqtziupn";
var serviceCollection = new ServiceCollection();
serviceCollection.AddEasyNetQ(AMQP).UseSystemTextJson();

using var provider = serviceCollection.BuildServiceProvider();
var bus = provider.GetRequiredService<IBus>();

int number = 0;
Console.WriteLine("Press any key to publish a message...");
while(true) {
	Console.ReadKey();
	var message = new Greeting($"Hello from {Environment.MachineName}", number++);
	await bus.PubSub.PublishAsync(message);
	Console.WriteLine($"Published: {message}");
}