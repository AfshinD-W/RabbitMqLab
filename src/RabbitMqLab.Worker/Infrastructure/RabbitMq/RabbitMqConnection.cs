using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqLab.Worker.Configs;

namespace RabbitMqLab.Worker.Infrastructure.RabbitMq;

public sealed class RabbitMqConnection : IHostedService
{
    private readonly RabbitMqConfigs _options;
    public IConnection? Connection { get; private set; }

    public RabbitMqConnection(IOptions<RabbitMqConfigs> options)
    {
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = "/",
            ClientProvidedName = "RabbitMqLab.Worker"
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
