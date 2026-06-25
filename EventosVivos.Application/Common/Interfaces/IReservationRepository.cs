using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<int> CountConfirmedAsync(Guid eventoId, CancellationToken ct = default);
    Task<int> CountLostAsync(Guid eventoId, CancellationToken ct = default);
    Task<decimal> SumRevenueAsync(Guid eventoId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> ListAllAsync(CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
}
