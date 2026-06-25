using MediatR;

namespace EventosVivos.Application.Reservations.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(Guid ReservationId) : IRequest<string>;
