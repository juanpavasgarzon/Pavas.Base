using SharedKernel;

namespace Infrastructure.Outbox;

public sealed class OutboxMessage : Entity
{
    public required OutboxMessageId Id { get; init; }
    public required string Type { get; init; }
    public required string Content { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
