using EventosVivos.Domain.Enums;
using MediatR;

namespace EventosVivos.Application.Models.Events;

public sealed record CreateEventCommand(
    string Title,
    string Description,
    int VenueId,
    int MaxCapacity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    decimal TicketPrice,
    EventType Type
) : IRequest<Guid>;
