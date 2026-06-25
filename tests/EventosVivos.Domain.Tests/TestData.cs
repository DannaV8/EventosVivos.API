using System.Reflection;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Tests;

internal static class TestData
{
    public static Venue Venue(int capacity = 200) =>
        new() { Id = 1, Name = "Auditorio Central", Capacity = capacity, City = "Bogotá" };

    public static DateTime NextWeekday(int hour = 18)
    {
        var d = DateTime.UtcNow.Date.AddDays(7);
        while (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            d = d.AddDays(1);
        return d.AddHours(hour);
    }

    public static DateTime NextSaturday(int hour)
    {
        var d = DateTime.UtcNow.Date.AddDays(7);
        while (d.DayOfWeek != DayOfWeek.Saturday)
            d = d.AddDays(1);
        return d.AddHours(hour);
    }

    public static Event ValidEvent(
        Venue? venue = null, int capacity = 100,
        decimal price = 50m, EventType type = EventType.Concert)
    {
        venue ??= Venue();
        var start = NextWeekday(18);
        return Event.Create(
            "Concierto de Jazz", "Una noche inolvidable de jazz en vivo.",
            venue, capacity, start, start.AddHours(3), price, type);
    }

    public static Event PastEvent()
    {
        var ev = (Event)Activator.CreateInstance(typeof(Event), nonPublic: true)!;
        Set(ev, "Id", Guid.NewGuid());
        Set(ev, "StartDateTime", DateTime.UtcNow.AddDays(-2));
        Set(ev, "EndDateTime", DateTime.UtcNow.AddDays(-1));
        return ev;
    }

    private static void Set(object target, string propertyName, object value)
    {
        var prop = target.GetType().GetProperty(
            propertyName, BindingFlags.Public | BindingFlags.Instance)!;
        prop.SetValue(target, value);
    }
    public static Reservation ConfirmedReservation()
    {
        var r = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2, "Ana", "ana@example.com");
        r.ConfirmPayment("EV-123456");
        return r;
    }
}
