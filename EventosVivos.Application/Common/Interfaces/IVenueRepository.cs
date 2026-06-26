using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default);
}
