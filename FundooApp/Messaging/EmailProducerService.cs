using System.Text;
using System.Text.Json;
using BuildingBlocks.Contracts.Messages;
using UserService.Application.Interfaces;
using RabbitMQ.Client;

namespace UserService.Api.Messaging;

public class EmailProducerService(IConfiguration configuration, ILogger<EmailProducerService> logger) : IEmailProducerService
{
    private IConnection? _connection;
    private IChannel? _channel;

    private async Task<IChannel> GetOrCreateChannelAsync(ConnectionFactory connectionFactory)
    {
        _connection ??= await connectionFactory.CreateConnectionAsync();
        _channel ??= await _connection.CreateChannelAsync();

        return _channel;
    }

    public async Task EnqueueAsync(EmailMessage message)
    {
        logger.LogInformation("Enqueue started to {email}", message.To);
        
        var connectionFactory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:HostName"]!,
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"]!,
            Password = configuration["RabbitMQ:Password"]!
        };

        await using var channel = await (_connection is { IsOpen: true } ? _connection : _connection = await connectionFactory.CreateConnectionAsync()).CreateChannelAsync();

        var queueName = configuration["RabbitMQ:QueueName"] ?? "fundoo-email-queue";
        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var jsonBody = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonBody);

        var properties = new BasicProperties()
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: true,
            basicProperties: properties,
            body: body
        );
        logger.LogInformation("Channel publish done");
    }
}
