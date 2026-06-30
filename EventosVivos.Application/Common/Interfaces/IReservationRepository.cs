using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<int> CountConfirmedAsync(Guid eventId, CancellationToken ct = default);
    Task<int> CountOnlyConfirmedAsync(Guid eventId, CancellationToken ct = default);
    Task<int> CountLostAsync(Guid eventId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> ListAllAsync(CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
}
