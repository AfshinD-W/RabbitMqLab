using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqLab.Worker.Infrastructure.RabbitMq;
using System.Text;

namespace RabbitMqLab.Worker.Services;

public class OrderConsumer : IHostedService
{
    private readonly RabbitMqConnection _rabbitConnection;
    private readonly ILogger<OrderConsumer> _logger;

    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

    public OrderConsumer(RabbitMqConnection rabbitConnection, ILogger<OrderConsumer> logger)
    {
        _rabbitConnection = rabbitConnection;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = _rabbitConnection.Connection
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

        _consumer = new AsyncEventingBasicConsumer(_channel);

        _consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation("Received: {Message}", message);

                if (message.Contains("Invalid"))
                {
                    await _channel.BasicRejectAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        requeue: false);

                    _logger.LogWarning("Message rejected: {Message}", message);
                    return;
                }

                // Simulate successful processing

                await _channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);

                _logger.LogInformation("Message processed successfully: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);

                await _channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "orders.queue",
            autoAck: false,
            consumer: _consumer,
            cancellationToken: cancellationToken
            );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }
    }
}
