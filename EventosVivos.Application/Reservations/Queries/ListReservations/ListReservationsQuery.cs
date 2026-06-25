using MediatR;

namespace EventosVivos.Application.Reservations.Queries.ListReservations;

public sealed record ListReservationsQuery(Guid UserId, bool IsAdmin = false) : IRequest<IReadOnlyList<ReservationDto>>;
