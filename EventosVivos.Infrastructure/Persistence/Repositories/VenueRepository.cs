using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public sealed class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _db;

    public VenueRepository(AppDbContext db) => _db = db;

    public async Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Venues.FindAsync([id], ct).AsTask();
}
