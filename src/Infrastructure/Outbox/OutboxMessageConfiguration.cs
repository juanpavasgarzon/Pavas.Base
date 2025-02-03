using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => OutboxMessageId.New(value));

        builder.Property(o => o.Content)
            .HasColumnType("jsonb");

        builder.HasIndex(o => new { o.OccurredOnUtc, o.ProcessedOnUtc })
            .HasFilter("processed_on_utc IS NULL")
            .IncludeProperties(o => new { o.Id, o.Type, o.Content });
    }
}
