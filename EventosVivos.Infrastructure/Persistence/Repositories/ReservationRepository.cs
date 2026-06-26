using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(AppDbContext db, ILogger<ReservationRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching reservation id={ReservationId}", id);
        return await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default) =>
        await _db.Reservations.AnyAsync(r => r.ReservationCode == code, ct);

    public async Task<int> CountConfirmedAsync(Guid eventId, CancellationToken ct = default) =>
        await _db.Reservations
            .Where(r => r.EventId == eventId && r.Status == ReservationStatus.Confirmed)
            .SumAsync(r => r.Quantity, ct);

    public async Task<int> CountLostAsync(Guid eventId, CancellationToken ct = default) =>
        await _db.Reservations
            .Where(r => r.EventId == eventId && r.IsLost)
            .SumAsync(r => r.Quantity, ct);

    public async Task<IReadOnlyList<Reservation>> ListByUserAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogDebug("Listing reservations for user={UserId}", userId);
        return await _db.Reservations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> ListAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Listing all reservations (admin)");
        return await _db.Reservations
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default) =>
        await _db.Reservations.AddAsync(reservation, ct);
}
