using System.Collections.Concurrent;
using EventosVivos.Application.Common.Interfaces;

namespace EventosVivos.Infrastructure.Locks;

public sealed class EventLockProvider : IEventLockProvider, IDisposable
{
    // Per-EventId SemaphoreSlim serializes concurrent reservation requests,
    // preventing overselling (race: read capacity → write reservation).
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<IAsyncDisposable> AcquireLockAsync(Guid eventId, CancellationToken ct)
    {
        var sem = _locks.GetOrAdd(eventId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        return new Releaser(sem);
    }

    public void Dispose()
    {
        foreach (var sem in _locks.Values)
            sem.Dispose();
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _sem;

        public Releaser(SemaphoreSlim sem) => _sem = sem;

        public ValueTask DisposeAsync()
        {
            _sem.Release();
            return ValueTask.CompletedTask;
        }
    }
}
