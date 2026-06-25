using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using MediatR;

namespace EventosVivos.Application.Reports.Queries.EventOccupancy;

public sealed class EventOccupancyHandler
    : IRequestHandler<EventOccupancyQuery, EventOccupancyDto>
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;

    public EventOccupancyHandler(IEventRepository events, IReservationRepository reservations)
    {
        _events = events;
        _reservations = reservations;
    }

    public async Task<EventOccupancyDto> Handle(
        EventOccupancyQuery query, CancellationToken ct)
    {
        var ev = await _events.GetByIdAsync(query.EventId, ct)
            ?? throw new NotFoundException($"Event {query.EventId} not found.");

        var confirmed = await _reservations.CountConfirmedAsync(query.EventId, ct);
        var lost = await _reservations.CountLostAsync(query.EventId, ct);
        var available = ev.MaxCapacity - confirmed - lost;
        var percentage = ev.MaxCapacity > 0
            ? Math.Round((double)confirmed / ev.MaxCapacity * 100, 2)
            : 0;
        var revenue = await _reservations.SumRevenueAsync(query.EventId, ct);

        return new EventOccupancyDto(
            ev.Id,
            ev.Title,
            SoldTickets: confirmed,
            AvailableTickets: available,
            OccupancyPercentage: percentage,
            TotalRevenue: revenue,
            Status: ev.Status.ToString());
    }
}
