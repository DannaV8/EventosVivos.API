using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Events;
using MediatR;

namespace EventosVivos.Application.Handlers.Events;

public sealed class GetEventByIdHandler
    : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;

    public GetEventByIdHandler(IEventRepository events, IReservationRepository reservations)
    {
        _events = events;
        _reservations = reservations;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery query, CancellationToken ct)
    {
        var ev = await _events.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException($"Event {query.Id} not found.");

        var confirmed = await _reservations.CountConfirmedAsync(ev.Id, ct);
        var lost = await _reservations.CountLostAsync(ev.Id, ct);

        return new EventDto(
            ev.Id,
            ev.Title,
            ev.Description,
            ev.VenueId,
            ev.Venue?.Name ?? string.Empty,
            ev.MaxCapacity,
            ev.StartDateTime,
            ev.EndDateTime,
            ev.TicketPrice,
            ev.Type.ToString(),
            ev.Status.ToString(),
            ConfirmedTickets: confirmed,
            LostTickets: lost);
    }
}
