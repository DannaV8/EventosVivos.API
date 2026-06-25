using EventosVivos.Domain.Enums;
using MediatR;

namespace EventosVivos.Application.Events.Queries.ListEvents;

public sealed record ListEventsQuery(
    EventType? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? Title = null
) : IRequest<IReadOnlyList<EventDto>>;
