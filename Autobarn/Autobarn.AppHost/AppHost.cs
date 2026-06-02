var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
	.WithContainerName("autobarn-rabbitmq")
	.WithManagementPlugin();

var website = builder.AddProject<Projects.Autobarn_Website>("autobarn-website")
	.WaitFor(rabbitmq)
	.WithReference(rabbitmq);

builder.AddProject<Projects.Autobarn_AuditLog>("auditlog")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.Build().Run();
