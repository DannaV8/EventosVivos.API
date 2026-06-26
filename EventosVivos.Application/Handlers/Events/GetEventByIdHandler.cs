using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Events;
using MediatR;

namespace EventosVivos.Application.Handlers.Events;

public sealed class GetEventByIdHandler
    : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IEventRepository _events;

    public GetEventByIdHandler(IEventRepository events) => _events = events;

    public async Task<EventDto> Handle(GetEventByIdQuery query, CancellationToken ct)
    {
        var ev = await _events.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException($"Event {query.Id} not found.");

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
            ev.Status.ToString());
    }
}
