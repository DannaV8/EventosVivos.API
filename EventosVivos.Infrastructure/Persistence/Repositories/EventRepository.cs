using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(AppDbContext db, ILogger<EventRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching event id={EventId}", id);
        return await _db.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken ct = default)
    {
        IQueryable<Event> query = _db.Events.Include(e => e.Venue);

        if (filter.Type is not null)
            query = query.Where(e => e.Type == filter.Type);
        if (filter.VenueId is not null)
            query = query.Where(e => e.VenueId == filter.VenueId);
        if (filter.StartDate is not null)
            query = query.Where(e => e.StartDateTime >= filter.StartDate);
        if (filter.EndDate is not null)
            query = query.Where(e => e.EndDateTime <= filter.EndDate);
        if (!string.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(e => EF.Functions.ILike(e.Title, $"%{filter.Title}%"));

        var events = await query.ToListAsync(ct);

        if (filter.Status is not null)
            events = events.Where(e => e.Status == filter.Status).ToList();

        _logger.LogDebug("ListAsync returned {Count} events", events.Count);
        return events;
    }

    public async Task<IReadOnlyList<Event>> GetActiveByVenueAsync(int venueId, CancellationToken ct = default)
    {
        var events = await _db.Events
            .Where(e => e.VenueId == venueId)
            .ToListAsync(ct);

        return events.Where(e => e.Status == EventStatus.Active).ToList();
    }

    public async Task AddAsync(Event ev, CancellationToken ct = default) =>
        await _db.Events.AddAsync(ev, ct);
}
