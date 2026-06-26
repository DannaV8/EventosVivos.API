using MediatR;

namespace EventosVivos.Application.Models.Events;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDto>;
