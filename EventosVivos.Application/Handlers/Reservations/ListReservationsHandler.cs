using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Reservations;
using MediatR;

namespace EventosVivos.Application.Handlers.Reservations;

public sealed class ListReservationsHandler : IRequestHandler<ListReservationsQuery, IReadOnlyList<ReservationDto>>
{
    private readonly IReservationRepository _reservations;
    private readonly IEventRepository _events;

    public ListReservationsHandler(IReservationRepository reservations, IEventRepository events)
    {
        _reservations = reservations;
        _events = events;
    }

    public async Task<IReadOnlyList<ReservationDto>> Handle(ListReservationsQuery query, CancellationToken ct)
    {
        var reservations = query.IsAdmin
            ? await _reservations.ListAllAsync(ct)
            : await _reservations.ListByUserAsync(query.UserId, ct);

        var result = new List<ReservationDto>(reservations.Count);
        foreach (var r in reservations)
        {
            var ev = await _events.GetByIdAsync(r.EventId, ct);
            result.Add(new ReservationDto(
                r.Id,
                r.EventId,
                ev?.Title ?? "Event not found",
                r.Quantity,
                r.Status.ToString(),
                r.ReservationCode,
                r.CreationDate));
        }

        return result;
    }
}
