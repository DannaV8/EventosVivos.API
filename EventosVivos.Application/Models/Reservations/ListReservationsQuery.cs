using MediatR;

namespace EventosVivos.Application.Models.Reservations;

public sealed record ListReservationsQuery(Guid UserId, bool IsAdmin = false) : IRequest<IReadOnlyList<ReservationDto>>;
