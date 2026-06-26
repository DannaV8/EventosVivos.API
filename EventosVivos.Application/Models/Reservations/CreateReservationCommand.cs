using MediatR;

namespace EventosVivos.Application.Models.Reservations;

public sealed record CreateReservationCommand(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    Guid UserId = default
) : IRequest<Guid>;
