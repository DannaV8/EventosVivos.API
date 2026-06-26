using EventosVivos.Domain.Enums;
using MediatR;

namespace EventosVivos.Application.Models.Events;

public sealed record ListEventsQuery(
    EventType? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? Title = null
) : IRequest<IReadOnlyList<EventDto>>;
