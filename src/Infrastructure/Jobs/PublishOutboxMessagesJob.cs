using Domain.Outbox;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using SharedKernel;

namespace Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class PublishOutboxMessagesJob(
    ApplicationDbContext applicationDbContext,
    ILogger<PublishOutboxMessagesJob> logger,
    IDateTimeProvider dateTimeProvider,
    IPublisher publisher
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        var outboxMessages = await applicationDbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var message in outboxMessages.TakeWhile(_ => !cancellationToken.IsCancellationRequested))
        {
            await HandleMessage(message, context.CancellationToken);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleMessage(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var @event = JsonConvert.DeserializeObject<IDomainEvent>(outboxMessage.Content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            if (@event is null)
            {
                return;
            }

            await publisher.Publish(@event, cancellationToken);
            outboxMessage.ProcessedOnUtc = dateTimeProvider.UtcNow;
        }
        catch (JsonException jsonEx)
        {
            logger.LogError(jsonEx, "Error processing outbox message.");

            outboxMessage.Error = $"Deserialization failed: {jsonEx.Message}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing outbox message.");

            outboxMessage.ProcessedOnUtc = dateTimeProvider.UtcNow;
            outboxMessage.Error = $"Error processing message: {ex.Message}";
        }
    }
}
