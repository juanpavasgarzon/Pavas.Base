using SharedKernel;

namespace Domain.Outbox;

public class OutboxMessageId(Guid value) : Identity<Guid>(value)
{
    public static OutboxMessageId New()
    {
        return new OutboxMessageId(Guid.NewGuid());
    }
}
