var builder = DistributedApplication.CreateBuilder(args);

var sqlSaPassword = builder.AddParameter("sql-sa-password", "p@ssw0rd", secret: true);

var sql = builder.AddSqlServer("sql", password: sqlSaPassword, port: 54321)
	.WithContainerName("autobarn-mssql-server")
	.WithLifetime(ContainerLifetime.Persistent)
	.AddDatabase("autobarn");

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
	.WithReference(rabbitmq)
	.WithReference(sql, connectionName: "AZURE_SQL_CONNECTIONSTRING");

var pricingServer = builder
	.AddProject<Projects.Autobarn_PricingServer>("autobarn-pricing-server")
	.WithHttpEndpoint(name: "grpc");

builder.AddProject<Projects.Autobarn_AuditLog>("auditlog")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.AddProject<Projects.Autobarn_PricingClient>("pricing-client")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq)
	.WithReference(pricingServer)
	.WaitFor(pricingServer)
	.WithEnvironment("grpc", pricingServer.GetEndpoint("grpc"));

builder.Build().Run();
