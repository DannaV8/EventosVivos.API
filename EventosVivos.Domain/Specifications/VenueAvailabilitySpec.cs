using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Specifications;

public sealed class VenueAvailabilitySpec
{
    public bool HasConflict(
        int venueId, DateTime start, DateTime end,
        IEnumerable<Event> activeEvents, Guid? excludeEventId = null)
    {
        return activeEvents
            .Where(e => e.VenueId == venueId)
            .Where(e => excludeEventId == null || e.Id != excludeEventId)
            .Where(e => e.Status == EventStatus.Active)
            .Any(e => start < e.EndDateTime && end > e.StartDateTime);
    }
}
