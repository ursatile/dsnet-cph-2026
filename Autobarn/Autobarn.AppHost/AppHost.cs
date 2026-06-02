var builder = DistributedApplication.CreateBuilder(args);

var rabbitMqUsername = builder.AddParameter("username", "admin", secret: true);
var rabbitMqPassword = builder.AddParameter("password", "secret", secret: true);

var rabbitmq = builder.AddRabbitMQ(
	"rabbitmq",
	userName: rabbitMqUsername,
	password: rabbitMqPassword)
	.WithContainerName("autobarn-rabbitmq")
	.WithManagementPlugin();

var website = builder.AddProject<Projects.Autobarn_Website>("autobarn-website")
	.WaitFor(rabbitmq)
	.WithReference(rabbitmq);

builder.AddProject<Projects.Autobarn_AuditLog>("auditlog")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.AddProject<Projects.Autobarn_PricingClient>("pricing-client")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.Build().Run();
