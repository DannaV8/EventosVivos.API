using MediatR;

namespace EventosVivos.Application.Reservations.Commands.CancelReservation;

public sealed record CancelReservationCommand(Guid ReservationId, Guid UserId, bool IsAdmin) : IRequest;
