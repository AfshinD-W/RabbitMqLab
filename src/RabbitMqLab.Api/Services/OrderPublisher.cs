using RabbitMQ.Client;
using RabbitMqLab.Api.Infrastructure.RabbitMq;
using System.Text;

namespace RabbitMqLab.Api.Services;

public sealed class OrderPublisher : IHostedService
{
    private readonly RabbitMqConnection _rabbitMqConnection;
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private IChannel? _channel;

    public OrderPublisher(RabbitMqConnection rabbitMqConnection)
    {
        _rabbitMqConnection = rabbitMqConnection;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = _rabbitMqConnection.Connection
            ?? throw new InvalidOperationException("RabbitMQ connection is not available.");

        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: "orders.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
            );
    }

    public async Task PublishAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");

        await _publishLock.WaitAsync(cancellationToken);

        try
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: "orders.queue",
                mandatory: true,
                body: body,
                cancellationToken: cancellationToken
                );
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        _publishLock.Release();
    }
}
