using Greetings;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5002");
var client = new Greeter.GreeterClient(channel);
Console.WriteLine("Press any key to send a request...");
Console.WriteLine("Press 1 for en-GB, 2 for en-US, 3 for en, 4 for da");
while (true)
{
	var languageCode = Console.ReadKey().Key switch {
		ConsoleKey.D1 => "en-GB",
		ConsoleKey.D2 => "en-US",
		ConsoleKey.D3 => "en",
		ConsoleKey.D4 => "da",
		_ => "en"
	};
	var reply = await client.SayHelloAsync(new HelloRequest { 
		FirstName = "NDC",
		LastName = "Copenhagen",
		LanguageCode = languageCode	});
	Console.WriteLine(reply.Message);
}


public enum LanguageCode
{
	Unknown = 0,
	EnGb,
	EnUs,
	En,
	Da
}
