using RabbitMqLab.Api.Configs;
using RabbitMqLab.Api.Infrastructure.RabbitMq;
using RabbitMqLab.Api.Services;

var builder = WebApplication.CreateBuilder(args);

//Configs
builder.Services.Configure<RabbitMqConfigs>(builder.Configuration.GetSection("RabbitMq"));


builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddHostedService(sb => sb.GetRequiredService<RabbitMqConnection>());

builder.Services.AddSingleton<OrderPublisher>();
builder.Services.AddHostedService(sb => sb.GetRequiredService<OrderPublisher>());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/publish", async (PublishMessageRequest request, OrderPublisher publisher, CancellationToken cancellationToken)
    =>
{
    await publisher.PublishAsync(request.Message, cancellationToken);

    return Results.Accepted(value: new
    {
        Message = "Message published successfully",
        Content = request.Message
    });
});

app.Run();

public sealed record PublishMessageRequest(string Message);