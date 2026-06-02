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
	.WithLifetime(ContainerLifetime.Persistent)
	.WithManagementPlugin();

var website = builder.AddProject<Projects.Autobarn_Website>("autobarn-website")
	//.WaitFor(rabbitmq)
	//.WithReference(rabbitmq)
	.WithHttpEndpoint(name: "autobarn-website-endpoint")
	.WithReference(sql, connectionName: "AZURE_SQL_CONNECTIONSTRING");

// uncomment this to use the.NET pricing server
var pricingServer = builder
   .AddProject<Projects.Autobarn_PricingServer>("autobarn-pricing-server")
   .WithHttpEndpoint(name: "grpc");

//var pricingServer = builder.AddPythonApp(
//	name: "autobarn-python-pricing-server",
//	appDirectory: "../../python",
//	scriptPath: "server.py")
//	.WithUv()
//	.WithHttpEndpoint(port: 5002, env: "PORT", name: "autobarn-grpc-server-http-endpoint");


builder.AddProject<Projects.Autobarn_AuditLog>("auditlog")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.AddProject<Projects.Autobarn_PricingClient>("pricing-client")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq)
	.WithReference(pricingServer)
	.WaitFor(pricingServer)
	.WithEnvironment("grpc", pricingServer.GetEndpoint("grpc"));

builder.AddProject<Projects.Autobarn_Notifier>("notifier")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq)
	.WithReference(website)
	.WithEnvironment("autobarn-website-url", website.GetEndpoint("autobarn-website-endpoint"))
	.WaitFor(website);

builder.Build().Run();
