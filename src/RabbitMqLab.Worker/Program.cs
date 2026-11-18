using RabbitMqLab.Worker.Configs;
using RabbitMqLab.Worker.Infrastructure.RabbitMq;
using RabbitMqLab.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqConfigs>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddHostedService(sb => sb.GetRequiredService<RabbitMqConnection>());

builder.Services.AddSingleton<OrderConsumer>();
builder.Services.AddHostedService(sb => sb.GetRequiredService<OrderConsumer>());

var host = builder.Build();
host.Run();
