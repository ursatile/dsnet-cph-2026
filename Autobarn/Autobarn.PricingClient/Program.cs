using Autobarn.PricingClient;
using Autobarn.PricingEngine;
using EasyNetQ;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var rabbitmq = builder.Configuration.GetConnectionString("rabbitmq");
builder.Services.AddEasyNetQ(rabbitmq);
builder.Services.AddHostedService<PricingClientService>();

var grpc = builder.Configuration["grpc"] ?? "http://localhost:5002";
var channel = GrpcChannel.ForAddress(grpc);
var pricerClient = new Pricer.PricerClient(channel);
builder.Services.AddSingleton(pricerClient);

var host = builder.Build();
var scope = host.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Connecting to pricing server on {grpc}", grpc);
host.Run();





