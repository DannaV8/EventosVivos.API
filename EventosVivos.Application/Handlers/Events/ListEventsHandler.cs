using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Common.Models;
using EventosVivos.Application.Models.Events;
using MediatR;

namespace EventosVivos.Application.Handlers.Events;

public sealed class ListEventsHandler
    : IRequestHandler<ListEventsQuery, PagedResult<EventDto>>
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;

    public ListEventsHandler(IEventRepository events, IReservationRepository reservations)
    {
        _events = events;
        _reservations = reservations;
    }

    public async Task<PagedResult<EventDto>> Handle(
        ListEventsQuery query, CancellationToken ct)
    {
        var filter = new EventFilter(
            Type: query.Type,
            StartDate: query.StartDate,
            EndDate: query.EndDate,
            VenueId: query.VenueId,
            Status: query.Status,
            Title: query.Title,
            Page: query.Page,
            PageSize: query.PageSize);

        var result = await _events.ListAsync(filter, ct);

        var items = new List<EventDto>();
        foreach (var e in result.Items)
        {
            var confirmed = await _reservations.CountConfirmedAsync(e.Id, ct);
            var lost = await _reservations.CountLostAsync(e.Id, ct);
            items.Add(new EventDto(
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
                e.Status.ToString(),
                ConfirmedTickets: confirmed,
                LostTickets: lost));
        }

        return new PagedResult<EventDto>(items, result.TotalCount, result.Page, result.PageSize);
    }
}
