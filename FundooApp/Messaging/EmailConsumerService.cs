using System.Text;
using System.Text.Json;
using BuildingBlocks.Contracts.Messages;
using UserService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace UserService.Api.Messaging
{
    public class EmailConsumerService(
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<EmailConsumerService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Consumer execute started");
            var connectionFactory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"]!,
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:UserName"]!,
                Password = configuration["RabbitMQ:Password"]!
            };

            await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);   
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var queueName = configuration["RabbitMQ:QueueName"] ?? "fundoo-email-queue";
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.Span);
                    var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message);

                    if (emailMessage is null ||
                    string.IsNullOrWhiteSpace(emailMessage.To) ||
                    string.IsNullOrWhiteSpace(emailMessage.Subject))
                    {
                        throw new JsonException("Invalid email message");
                    }


                    await emailService.SendEmailAsync(emailMessage.To, emailMessage.Subject, emailMessage.Body);

                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);

                    logger.LogInformation("Email sent successfully to {Email}", emailMessage.To);

                } catch(Exception e)
                {
                    logger.LogError(e, "Failed to process email message");

                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            };
            
            await channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);
            logger.LogInformation("RabbitMQ email consumer started");

            await Task.Delay(
            Timeout.Infinite,
            cancellationToken);
        }
    }
}