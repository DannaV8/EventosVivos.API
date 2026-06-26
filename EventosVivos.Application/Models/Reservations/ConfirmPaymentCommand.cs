using MediatR;

namespace EventosVivos.Application.Models.Reservations;

public sealed record ConfirmPaymentCommand(Guid ReservationId) : IRequest<string>;
