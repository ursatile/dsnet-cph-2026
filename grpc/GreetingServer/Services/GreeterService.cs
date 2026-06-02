using Greetings;
using Grpc.Core;

namespace GreetingServer.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
	public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
	{
		var greeting = request.LanguageCode switch
		{
			"en-GB" => "Good Morning",
			"en-US" => "Howdy!",
			"en" => "Hello",
			"da" => "Hej",
			_ => "Hello"
		};
		
		logger.LogInformation("The message is received from {Name}", request.Name);

		return Task.FromResult(new HelloReply
		{
			Message = greeting + " " + request.Name
		});
	}
}
