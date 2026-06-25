using MediatR;

namespace EventosVivos.Application.Reservations.Commands.CreateReservation;

public sealed record CreateReservationCommand(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    Guid UserId = default
) : IRequest<Guid>;
