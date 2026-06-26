namespace EventosVivos.Application.Common.Interfaces;

public interface IEventLockProvider
{
    Task<IAsyncDisposable> AcquireLockAsync(Guid eventId, CancellationToken ct);
}
