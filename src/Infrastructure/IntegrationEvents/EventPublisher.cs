using Application.Abstractions.IntegrationEvents;
using MassTransit;

namespace Infrastructure.IntegrationEvents;

public class EventPublisher(IPublishEndpoint publishEndpoint): IEventPublisher
{
    public async Task Publish<T>(T @event, CancellationToken cancellationToken) where T : class
    {
        await publishEndpoint.Publish(@event, cancellationToken);
    }
}
