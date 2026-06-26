using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Common.Interfaces;

public sealed record EventFilter(
    EventType? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? VenueId = null,
    EventStatus? Status = null,
    string? Title = null,
    int Page = 1,
    int PageSize = 9
);
