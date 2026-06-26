using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Reservations;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.Handlers.Reservations;

public sealed class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;
    private readonly IUnitOfWork _uow;
    private readonly ReservationValidationService _validationService;
    private readonly IEventLockProvider _lockProvider;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(
        IEventRepository events, IReservationRepository reservations,
        IUnitOfWork uow, ReservationValidationService validationService,
        IEventLockProvider lockProvider,
        ILogger<CreateReservationHandler> logger)
    {
        _events = events;
        _reservations = reservations;
        _uow = uow;
        _validationService = validationService;
        _lockProvider = lockProvider;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateReservationCommand cmd, CancellationToken ct)
    {
        // Serialize per-event to prevent overselling under concurrent requests.
        await using var @lock = await _lockProvider.AcquireLockAsync(cmd.EventId, ct);

        _logger.LogInformation(
            "Creating reservation: event={EventId} user={UserId} quantity={Quantity}",
            cmd.EventId, cmd.UserId, cmd.Quantity);

        var ev = await _events.GetByIdAsync(cmd.EventId, ct)
            ?? throw new NotFoundException($"Event {cmd.EventId} not found.");

        var confirmed = await _reservations.CountConfirmedAsync(cmd.EventId, ct);
        var lost = await _reservations.CountLostAsync(cmd.EventId, ct);

        _validationService.Validate(
            ev.Status, cmd.Quantity, ev.TicketPrice, ev.StartDateTime,
            ev.MaxCapacity, confirmed, lost);

        var reservation = Reservation.Create(
            cmd.EventId, cmd.UserId, cmd.Quantity,
            cmd.BuyerName, cmd.BuyerEmail);

        await _reservations.AddAsync(reservation, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Reservation created: id={ReservationId}", reservation.Id);
        return reservation.Id;
    }
}
