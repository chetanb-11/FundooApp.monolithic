namespace BuildingBlocks.Contracts.Messages;

/// <summary>
/// Message published to RabbitMQ (fundoo-email-queue) and consumed by the email consumer.
/// Lives in Contracts so NotificationService can consume it in phase 2.
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
