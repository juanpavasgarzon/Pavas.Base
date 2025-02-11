namespace Application.Abstractions.IntegrationEvents;

public interface IEventPublisher
{
    Task Publish<T>(T @event, CancellationToken cancellationToken) where T: class;
}
