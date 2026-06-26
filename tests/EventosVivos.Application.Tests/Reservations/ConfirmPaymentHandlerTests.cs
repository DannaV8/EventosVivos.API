using System.Text.RegularExpressions;
using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Handlers.Reservations;
using EventosVivos.Application.Models.Reservations;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventosVivos.Application.Tests.Reservations;

public class ConfirmPaymentHandlerTests : HandlerTestBase
{
    private ConfirmPaymentHandler CreateHandler() =>
        new(Reservations, Db, NullLogger<ConfirmPaymentHandler>.Instance);

    private async Task<Reservation> PersistReservationAsync(Reservation reservation)
    {
        Db.Reservations.Add(reservation);
        await Db.SaveChangesAsync();
        return reservation;
    }

    [Fact]
    public async Task Generates_Code_Format_EV_6Digits()
    {
        var reservation = await PersistReservationAsync(
            Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2, "Ana", "ana@example.com"));

        var code = await CreateHandler().Handle(
            new ConfirmPaymentCommand(reservation.Id), CancellationToken.None);

        Assert.Matches(new Regex(@"^EV-\d{6}$"), code);

        var persisted = await Db.Reservations.FindAsync(reservation.Id);
        Assert.Equal(ReservationStatus.Confirmed, persisted!.Status);
        Assert.Equal(code, persisted.ReservationCode);
    }

    [Fact]
    public async Task Rejects_If_Already_Confirmed()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2, "Ana", "ana@example.com");
        reservation.ConfirmPayment("EV-123456");
        await PersistReservationAsync(reservation);

        var ex = await Assert.ThrowsAsync<InvalidReservationStateException>(() =>
            CreateHandler().Handle(new ConfirmPaymentCommand(reservation.Id), CancellationToken.None));
        Assert.Equal("RESERVATION_ALREADY_CONFIRMED", ex.Code);
    }

    [Fact]
    public async Task Reservation_NotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new ConfirmPaymentCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
