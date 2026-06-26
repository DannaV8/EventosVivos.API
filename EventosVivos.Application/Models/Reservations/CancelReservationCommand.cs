using MediatR;

namespace EventosVivos.Application.Models.Reservations;

public sealed record CancelReservationCommand(Guid ReservationId, Guid UserId, bool IsAdmin) : IRequest;
