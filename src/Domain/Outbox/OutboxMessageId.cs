using SharedKernel;

namespace Domain.Outbox;

public class OutboxMessageId : ValueObject
{
    private OutboxMessageId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static OutboxMessageId New(Guid id)
    {
        return new OutboxMessageId(id);
    }
    
    public static OutboxMessageId Create()
    {
        return new OutboxMessageId(Guid.NewGuid());
    }
}
