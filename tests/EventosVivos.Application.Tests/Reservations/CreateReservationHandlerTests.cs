using EventosVivos.Application.Handlers.Reservations;
using EventosVivos.Application.Models.Reservations;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventosVivos.Application.Tests.Reservations;

public class CreateReservationHandlerTests : HandlerTestBase
{
    private CreateReservationHandler CreateHandler() =>
        new(Events, Reservations, Db, Validation, LockProvider, NullLogger<CreateReservationHandler>.Instance);

    private async Task<Event> PersistEventAsync(Event ev)
    {
        Db.Events.Add(ev);
        await Db.SaveChangesAsync();
        return ev;
    }

    private async Task<Venue> GetVenueAsync(int id = 1) =>
        (await Venues.GetByIdAsync(id))!;

    [Fact]
    public async Task Reservation_Successful_Creates_PendingPayment_Record()
    {
        var venue = await GetVenueAsync();
        var ev = await PersistEventAsync(TestData.ValidEvent(venue, capacity: 100));

        var cmd = new CreateReservationCommand(ev.Id, 2, "Ana", "ana@example.com", Guid.NewGuid());
        var id = await CreateHandler().Handle(cmd, CancellationToken.None);

        var reservation = await Db.Reservations.FindAsync(id);
        Assert.NotNull(reservation);
        Assert.Equal(ReservationStatus.PendingPayment, reservation!.Status);
        Assert.Null(reservation.ReservationCode);
    }

    [Fact]
    public async Task Does_Not_Allow_Exceeding_Capacity()
    {
        var venue = await GetVenueAsync();
        var ev = await PersistEventAsync(TestData.ValidEvent(venue, capacity: 5));

        var cmd = new CreateReservationCommand(ev.Id, 6, "Ana", "ana@example.com", Guid.NewGuid());

        await Assert.ThrowsAsync<CapacityExceededException>(() =>
            CreateHandler().Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Does_Not_Allow_Reserving_Under_1h_Before()
    {
        var start = DateTime.UtcNow.AddMinutes(30);
        var ev = await PersistEventAsync(
            TestData.HydratedEvent(venueId: 1, start: start, end: start.AddHours(2), capacity: 100));

        var cmd = new CreateReservationCommand(ev.Id, 1, "Ana", "ana@example.com", Guid.NewGuid());

        await Assert.ThrowsAsync<EventSoonException>(() =>
            CreateHandler().Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Lost_Tickets_Do_Not_Count_As_Available()
    {
        var venue = await GetVenueAsync();
        var ev = await PersistEventAsync(TestData.ValidEvent(venue, capacity: 5));

        var r1 = Reservation.Create(ev.Id, Guid.NewGuid(), 3, "Ana", "ana@example.com");
        r1.ConfirmPayment("EV-111111");
        var r2 = Reservation.Create(ev.Id, Guid.NewGuid(), 2, "Beto", "beto@example.com");
        r2.ConfirmPayment("EV-222222");
        r2.Cancel(DateTime.UtcNow.AddHours(24));
        Db.Reservations.AddRange(r1, r2);
        await Db.SaveChangesAsync();

        var cmd = new CreateReservationCommand(ev.Id, 1, "Caro", "caro@example.com", Guid.NewGuid());

        await Assert.ThrowsAsync<CapacityExceededException>(() =>
            CreateHandler().Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Rule_24h_Takes_Priority_Over_Price_Rule()
    {
        var start = DateTime.UtcNow.AddHours(10);
        var ev = await PersistEventAsync(
            TestData.HydratedEvent(venueId: 1, start: start, end: start.AddHours(2),
                capacity: 100, price: 200m));

        var cmd = new CreateReservationCommand(ev.Id, 6, "Ana", "ana@example.com", Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<InvalidEventException>(() =>
            CreateHandler().Handle(cmd, CancellationToken.None));
        Assert.Equal("LIMIT_24H", ex.Code);
    }

    [Fact]
    public async Task Event_NotActive_ThrowsInvalidEvent()
    {
        var venue = await GetVenueAsync();
        var ev = TestData.ValidEvent(venue, capacity: 100);
        ev.Cancel(); 
        await PersistEventAsync(ev);

        var cmd = new CreateReservationCommand(ev.Id, 1, "Ana", "ana@example.com", Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<InvalidEventException>(() =>
            CreateHandler().Handle(cmd, CancellationToken.None));
        Assert.Equal("EVENT_NOT_ACTIVE", ex.Code);
    }
}
