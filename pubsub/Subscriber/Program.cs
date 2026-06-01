using EasyNetQ;
using Messages;
using Microsoft.Extensions.DependencyInjection;

const string AMQP = "amqps://yqtziupn:0TYNVJ3F3JTBd6BWSAh_3jIAYW95AfDJ@sleepy-charcoal-alpaca.rmq7.cloudamqp.com/yqtziupn";
var serviceCollection = new ServiceCollection();
serviceCollection.AddEasyNetQ(AMQP).UseSystemTextJson();

using var provider = serviceCollection.BuildServiceProvider();
var bus = provider.GetRequiredService<IBus>();

Console.WriteLine("Subscribing to messages...");
await bus.PubSub.SubscribeAsync<Greeting>("subscriber", message => {
    Console.WriteLine($"Received: {message}");
});

Console.WriteLine("Press any key to exit...");
Console.ReadKey();