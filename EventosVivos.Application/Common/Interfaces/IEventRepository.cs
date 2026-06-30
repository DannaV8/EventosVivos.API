using EventosVivos.Application.Common.Models;
using EventosVivos.Application.Models.Reports;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Event>> ListAsync(EventFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<Event>> GetActiveByVenueAsync(int venueId, CancellationToken ct = default);
    Task AddAsync(Event ev, CancellationToken ct = default);
    Task<IReadOnlyList<EventOccupancyDto>> ListOccupanciesAsync(CancellationToken ct = default);
}
