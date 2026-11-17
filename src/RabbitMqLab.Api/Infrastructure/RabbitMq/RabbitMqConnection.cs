using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqLab.Api.Configs;

namespace RabbitMqLab.Api.Infrastructure.RabbitMq;

public sealed class RabbitMqConnection : IHostedService
{
    private readonly RabbitMqConfigs _configs;
    public IConnection? Connection { get; private set; }

    public RabbitMqConnection(IOptions<RabbitMqConfigs> configs)
    {
        _configs = configs.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configs.HostName,
            Port = _configs.Port,
            UserName = _configs.UserName,
            Password = _configs.Password,
            VirtualHost = "/",
            ClientProvidedName = "RabbitMqLab.Api",
        };

        Connection = await factory.CreateConnectionAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Connection is not null)
        {
            await Connection.DisposeAsync();
            Connection = null;
        }
    }
}