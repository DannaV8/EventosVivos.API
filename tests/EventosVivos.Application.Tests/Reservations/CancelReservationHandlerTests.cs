using EventosVivos.Application.Reservations.Commands.CancelReservation;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventosVivos.Application.Tests.Reservations;

public class CancelReservationHandlerTests : HandlerTestBase
{
    private CancelReservationHandler CreateHandler() =>
        new(Reservations, Events, Db, NullLogger<CancelReservationHandler>.Instance);

    private async Task<Reservation> SetupConfirmedAsync(DateTime eventStart)
    {
        var ev = TestData.HydratedEvent(
            venueId: 1, start: eventStart, end: eventStart.AddHours(2));
        var reservation = Reservation.Create(ev.Id, Guid.NewGuid(), 2, "Ana", "ana@example.com");
        reservation.ConfirmPayment("EV-123456");

        Db.Events.Add(ev);
        Db.Reservations.Add(reservation);
        await Db.SaveChangesAsync();
        return reservation;
    }

    [Fact]
    public async Task Cancel_Confirmed_Over48h_NotLost()
    {
        var reservation = await SetupConfirmedAsync(DateTime.UtcNow.AddHours(72));

        await CreateHandler().Handle(
            new CancelReservationCommand(reservation.Id, reservation.UserId, false),
            CancellationToken.None);

        var persisted = await Db.Reservations.FindAsync(reservation.Id);
        Assert.Equal(ReservationStatus.Cancelled, persisted!.Status);
        Assert.False(persisted.IsLost);
    }

    [Fact]
    public async Task Cancel_Confirmed_Under48h_MarksLost()
    {
        var reservation = await SetupConfirmedAsync(DateTime.UtcNow.AddHours(24));

        await CreateHandler().Handle(
            new CancelReservationCommand(reservation.Id, reservation.UserId, false),
            CancellationToken.None);

        var persisted = await Db.Reservations.FindAsync(reservation.Id);
        Assert.Equal(ReservationStatus.Cancelled, persisted!.Status);
        Assert.True(persisted.IsLost); 
    }

    [Fact]
    public async Task Cancel_PendingPayment_ThrowsInvalidState()
    {
        var eventStart = DateTime.UtcNow.AddHours(72);
        var ev = TestData.HydratedEvent(
            venueId: 1, start: eventStart, end: eventStart.AddHours(2));
        var reservation = Reservation.Create(ev.Id, Guid.NewGuid(), 2, "Ana", "ana@example.com");

        Db.Events.Add(ev);
        Db.Reservations.Add(reservation);
        await Db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidReservationStateException>(() =>
            CreateHandler().Handle(
                new CancelReservationCommand(reservation.Id, reservation.UserId, false),
                CancellationToken.None));
    }
}
