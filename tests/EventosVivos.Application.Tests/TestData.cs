using System.Reflection;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Tests;
internal static class TestData
{
    public static DateTime NextWeekday(int hour = 18)
    {
        var d = DateTime.UtcNow.Date.AddDays(7);
        while (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            d = d.AddDays(1);
        return d.AddHours(hour);
    }

    public static Event ValidEvent(
        Venue venue, int capacity = 100, decimal price = 50m,
        DateTime? start = null, EventType type = EventType.Concert)
    {
        var startDate = start ?? NextWeekday(18);
        return Event.Create(
            "Jazz Concert", "An unforgettable night of live jazz.",
            venue, capacity, startDate, startDate.AddHours(3), price, type);
    }

    public static Event HydratedEvent(
        int venueId, DateTime start, DateTime end,
        int capacity = 100, decimal price = 50m, EventType type = EventType.Concert)
    {
        var ev = (Event)Activator.CreateInstance(typeof(Event), nonPublic: true)!;
        Set(ev, nameof(Event.Id), Guid.NewGuid());
        Set(ev, nameof(Event.Title), "Test event");
        Set(ev, nameof(Event.Description), "Sufficient test description.");
        Set(ev, nameof(Event.VenueId), venueId);
        Set(ev, nameof(Event.MaxCapacity), capacity);
        Set(ev, nameof(Event.StartDateTime), start);
        Set(ev, nameof(Event.EndDateTime), end);
        Set(ev, nameof(Event.TicketPrice), price);
        Set(ev, nameof(Event.Type), type);
        return ev;
    }

    private static void Set(object target, string propertyName, object value)
    {
        var prop = target.GetType().GetProperty(
            propertyName, BindingFlags.Public | BindingFlags.Instance)!;
        prop.SetValue(target, value);
    }
}
