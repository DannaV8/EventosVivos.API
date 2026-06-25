using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using Xunit;

namespace EventosVivos.Domain.Tests;

public class EventTests
{
    [Fact]
    public void Create_WithValidData_CreatesEvent()
    {
        var venue = TestData.Venue(capacity: 200);
        var start = TestData.NextWeekday(18);

        var ev = Domain.Entities.Event.Create(
            "Concierto", "Descripción suficientemente larga.",
            venue, 150, start, start.AddHours(2), 80m, EventType.Concert);

        Assert.NotEqual(Guid.Empty, ev.Id);
        Assert.Equal(venue.Id, ev.VenueId);
        Assert.Equal(150, ev.MaxCapacity);
        Assert.Equal(EventStatus.Active, ev.Status);
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("")]
    public void Create_WithInvalidTitle_Throws(string title)
    {
        var venue = TestData.Venue();
        var start = TestData.NextWeekday(18);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            title, "Descripción válida y larga.", venue, 100, start, start.AddHours(2), 50m, EventType.Workshop));
        Assert.Equal("INVALID_TITLE", ex.Code);
    }

    [Fact]
    public void Create_WithTitleTooLong_Throws()
    {
        var venue = TestData.Venue();
        var start = TestData.NextWeekday(18);
        var title = new string('a', 101);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            title, "Descripción válida y larga.", venue, 100, start, start.AddHours(2), 50m, EventType.Workshop));
        Assert.Equal("INVALID_TITLE", ex.Code);
    }

    [Fact]
    public void Create_WithShortDescription_Throws()
    {
        var venue = TestData.Venue();
        var start = TestData.NextWeekday(18);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            "Titulo válido", "corta", venue, 100, start, start.AddHours(2), 50m, EventType.Workshop));
        Assert.Equal("INVALID_DESCRIPTION", ex.Code);
    }


    [Fact]
    public void Create_CapacityGreaterThanVenue_Throws()
    {
        var venue = TestData.Venue(capacity: 50);
        var start = TestData.NextWeekday(18);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 51, start, start.AddHours(2), 50m, EventType.Workshop));
        Assert.Equal("INVALID_CAPACITY", ex.Code);
    }

    [Fact]
    public void Create_CapacityEqualsVenue_IsValid()
    {
        var venue = TestData.Venue(capacity: 50);
        var start = TestData.NextWeekday(18);

        var ev = Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 50, start, start.AddHours(2), 50m, EventType.Workshop);
        Assert.Equal(50, ev.MaxCapacity);
    }

    [Fact]
    public void Create_ZeroCapacity_Throws()
    {
        var venue = TestData.Venue();
        var start = TestData.NextWeekday(18);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 0, start, start.AddHours(2), 50m, EventType.Workshop));
        Assert.Equal("INVALID_CAPACITY", ex.Code);
    }


    [Fact]
    public void Create_PastStart_Throws()
    {
        var venue = TestData.Venue();
        var past = DateTime.UtcNow.AddHours(-1);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 100, past, past.AddHours(2), 50m, EventType.Workshop));
        Assert.Equal("PAST_EVENT", ex.Code);
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        var venue = TestData.Venue();
        var start = TestData.NextWeekday(18);

        var ex = Assert.Throws<InvalidEventException>(() => Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 100, start, start.AddHours(-1), 50m, EventType.Workshop));
        Assert.Equal("INVALID_END_DATE", ex.Code);
    }

    [Fact]
    public void Create_SaturdayAfter10PM_Throws()
    {
        var venue = TestData.Venue();
        var start = TestData.NextSaturday(22);

        var ex = Assert.Throws<NightScheduleException>(() => Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 100, start, start.AddHours(2), 50m, EventType.Concert));
        Assert.Equal("NIGHT_TIME", ex.Code);
    }

    [Fact]
    public void Create_SaturdayBefore10PM_IsValid()
    {
        var venue = TestData.Venue();
        var start = TestData.NextSaturday(21);

        var ev = Domain.Entities.Event.Create(
            "Titulo válido", "Descripción válida y larga.", venue, 100, start, start.AddHours(2), 50m, EventType.Concert);
        Assert.Equal(EventStatus.Active, ev.Status);
    }

    [Fact]
    public void Status_NewEvent_IsActive()
    {
        var ev = TestData.ValidEvent();
        Assert.Equal(EventStatus.Active, ev.Status);
    }

    [Fact]
    public void Status_AfterCancel_IsCancelled()
    {
        var ev = TestData.ValidEvent();
        ev.Cancel();
        Assert.Equal(EventStatus.Cancelled, ev.Status);
    }

    [Fact]
    public void Status_WithPastEndDate_IsCompleted()
    {
        var ev = TestData.PastEvent();
        Assert.Equal(EventStatus.Completed, ev.Status);
    }

    [Fact]
    public void Status_CancelledHasPriorityOverCompleted()
    {
        var ev = TestData.PastEvent();
        ev.Cancel();
        Assert.Equal(EventStatus.Cancelled, ev.Status);
    }
}
