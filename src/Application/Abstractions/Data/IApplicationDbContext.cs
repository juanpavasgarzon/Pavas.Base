namespace Application.Abstractions.Data;

public interface IApplicationDbContext : IDisposable, IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
