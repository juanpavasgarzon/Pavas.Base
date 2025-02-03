using MediatR;

namespace SharedKernel;

public interface IDomainEvent : INotification
{
    public Guid Id { get; init; }
}
