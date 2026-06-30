using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Reservations;
using EventosVivos.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.Handlers.Reservations;

public sealed class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, string>
{
    private readonly IReservationRepository _reservations;
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ConfirmPaymentHandler> _logger;

    public ConfirmPaymentHandler(
        IReservationRepository reservations, IEventRepository events,
        IUnitOfWork uow, ILogger<ConfirmPaymentHandler> logger)
    {
        _reservations = reservations;
        _events = events;
        _uow = uow;
        _logger = logger;
    }

    public async Task<string> Handle(ConfirmPaymentCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Confirming payment for reservation: id={ReservationId}", cmd.ReservationId);

        var reservation = await _reservations.GetByIdAsync(cmd.ReservationId, ct)
            ?? throw new NotFoundException($"Reservation {cmd.ReservationId} not found.");

        var ev = await _events.GetByIdAsync(reservation.EventId, ct)
            ?? throw new NotFoundException($"Event {reservation.EventId} not found.");

        // Count only already-confirmed tickets (excluding this reservation which is still Pending).
        var alreadyConfirmed = await _reservations.CountOnlyConfirmedAsync(reservation.EventId, ct);
        var lost = await _reservations.CountLostAsync(reservation.EventId, ct);
        var available = ev.MaxCapacity - alreadyConfirmed - lost;

        if (reservation.Quantity > available)
            throw new CapacityExceededException(reservation.Quantity, available);

        string code;
        var attempts = 0;
        do
        {
            if (++attempts > 10)
                throw new InvalidOperationException("Could not generate a unique reservation code. Please try again.");
            code = $"EV-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _reservations.CodeExistsAsync(code, ct));

        reservation.ConfirmPayment(code);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Reservation {ReservationId} confirmed with code {Code}",
            reservation.Id, code);
        return code;
    }
}
