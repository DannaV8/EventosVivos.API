using EventosVivos.Application.Common.Interfaces;
using MediatR;

namespace EventosVivos.Application.Events.Queries.ListEvents;

public sealed class ListEventsHandler
    : IRequestHandler<ListEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IEventRepository _events;

    public ListEventsHandler(IEventRepository events) => _events = events;

    public async Task<IReadOnlyList<EventDto>> Handle(
        ListEventsQuery query, CancellationToken ct)
    {
        var filter = new EventFilter(
            Type: query.Type,
            StartDate: query.StartDate,
            EndDate: query.EndDate,
            VenueId: query.VenueId,
            Status: query.Status,
            Title: query.Title);

        var events = await _events.ListAsync(filter, ct);

        return events
            .Select(e => new EventDto(
                e.Id,
                e.Title,
                e.Description,
                e.VenueId,
                e.Venue?.Name ?? string.Empty,
                e.MaxCapacity,
                e.StartDateTime,
                e.EndDateTime,
                e.TicketPrice,
                e.Type.ToString(),
                e.Status.ToString()))
            .ToList();
    }
}
