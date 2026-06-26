using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Models;
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

    public async Task<PagedResult<Event>> ListAsync(EventFilter filter, CancellationToken ct = default)
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

        // Stable ordering is required for deterministic pagination.
        query = query.OrderBy(e => e.StartDateTime);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 9 : filter.PageSize;

        // Event.Status is computed (not a DB column), so when filtering by status
        // we must materialize first, then filter and paginate in memory.
        if (filter.Status is not null)
        {
            var all = await query.ToListAsync(ct);
            var filtered = all.Where(e => e.Status == filter.Status).ToList();

            var pageItems = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogDebug("ListAsync (status filter) returned {Count}/{Total}", pageItems.Count, filtered.Count);
            return new PagedResult<Event>(pageItems, filtered.Count, page, pageSize);
        }

        // No status filter → true DB-level pagination (efficient).
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        _logger.LogDebug("ListAsync returned {Count}/{Total}", items.Count, totalCount);
        return new PagedResult<Event>(items, totalCount, page, pageSize);
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
