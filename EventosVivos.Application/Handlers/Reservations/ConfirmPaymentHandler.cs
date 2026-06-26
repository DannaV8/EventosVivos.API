using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Reservations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.Handlers.Reservations;

public sealed class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, string>
{
    private readonly IReservationRepository _reservations;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ConfirmPaymentHandler> _logger;

    public ConfirmPaymentHandler(
        IReservationRepository reservations, IUnitOfWork uow,
        ILogger<ConfirmPaymentHandler> logger)
    {
        _reservations = reservations;
        _uow = uow;
        _logger = logger;
    }

    public async Task<string> Handle(ConfirmPaymentCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Confirming payment for reservation: id={ReservationId}", cmd.ReservationId);

        var reservation = await _reservations.GetByIdAsync(cmd.ReservationId, ct)
            ?? throw new NotFoundException($"Reservation {cmd.ReservationId} not found.");

        string code;
        do { code = $"EV-{Random.Shared.Next(100000, 999999)}"; }
        while (await _reservations.CodeExistsAsync(code, ct));

        reservation.ConfirmPayment(code);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Reservation {ReservationId} confirmed with code {Code}",
            reservation.Id, code);
        return code;
    }
}
