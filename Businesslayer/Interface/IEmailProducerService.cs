using BuildingBlocks.Contracts.Messages;

namespace UserService.Application.Interfaces;

public interface IEmailProducerService
{
    Task EnqueueAsync(EmailMessage message);
}
