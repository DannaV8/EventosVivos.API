using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Handlers.Events;
using EventosVivos.Application.Models.Events;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Application.Tests.Events;

public class CreateEventHandlerTests : HandlerTestBase
{
    private CreateEventHandler CreateHandler() => new(Events, Venues, Db);

    private CreateEventCommand ValidCommand(int venueId = 1, int capacity = 100,
        DateTime? start = null) =>
        new("Jazz Festival", "A great night of live music.",
            venueId, capacity,
            start ?? TestData.NextWeekday(18),
            (start ?? TestData.NextWeekday(18)).AddHours(3),
            50m, EventType.Concert);

    [Fact]
    public async Task Event_ValidData_Persists()
    {
        var id = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.True(await Db.Events.AnyAsync(e => e.Id == id));
    }

    [Fact]
    public async Task Venue_NotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(ValidCommand(venueId: 999), CancellationToken.None));
    }

    [Fact]
    public async Task Venue_WithOverlappingEvent_ThrowsVenueConflict()
    {
        var start = TestData.NextWeekday(18);
        await CreateHandler().Handle(ValidCommand(venueId: 1, start: start), CancellationToken.None);
        await Assert.ThrowsAsync<VenueConflictException>(() =>
            CreateHandler().Handle(
                ValidCommand(venueId: 1, start: start.AddHours(1)), CancellationToken.None));
    }

    [Fact]
    public async Task Capacity_ExceedsVenue_ThrowsInvalidEvent()
    {
        var ex = await Assert.ThrowsAsync<InvalidEventException>(() =>
            CreateHandler().Handle(ValidCommand(venueId: 1, capacity: 201), CancellationToken.None));

        Assert.Equal("INVALID_CAPACITY", ex.Code);
    }
}
