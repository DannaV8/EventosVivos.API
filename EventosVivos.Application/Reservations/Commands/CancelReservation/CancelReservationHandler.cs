using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.Reservations.Commands.CancelReservation;

public sealed class CancelReservationHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IReservationRepository _reservations;
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CancelReservationHandler> _logger;

    public CancelReservationHandler(
        IReservationRepository reservations, IEventRepository events,
        IUnitOfWork uow, ILogger<CancelReservationHandler> logger)
    {
        _reservations = reservations;
        _events = events;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(CancelReservationCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation(
            "Cancelling reservation: id={ReservationId} requestedBy={UserId} isAdmin={IsAdmin}",
            cmd.ReservationId, cmd.UserId, cmd.IsAdmin);

        var reservation = await _reservations.GetByIdAsync(cmd.ReservationId, ct)
            ?? throw new NotFoundException($"Reservation {cmd.ReservationId} not found.");

        if (!cmd.IsAdmin && reservation.UserId != cmd.UserId)
        {
            _logger.LogWarning(
                "Cancellation denied: reservation {ReservationId} belongs to {OwnerId}, requested by {UserId}",
                cmd.ReservationId, reservation.UserId, cmd.UserId);
            throw new InvalidEventException("RESERVATION_NOT_OWNED",
                "You cannot cancel a reservation that is not yours.");
        }

        var ev = await _events.GetByIdAsync(reservation.EventId, ct)
            ?? throw new NotFoundException($"Event {reservation.EventId} not found.");

        reservation.Cancel(ev.StartDateTime);

        if (reservation.IsLost)
            _logger.LogWarning(
                "RN-07 applied: reservation {ReservationId} cancelled with penalty (< 48h until event {EventId})",
                reservation.Id, ev.Id);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Reservation {ReservationId} cancelled", reservation.Id);
    }
}
