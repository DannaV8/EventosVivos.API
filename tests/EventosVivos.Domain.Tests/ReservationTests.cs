using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using Xunit;

namespace EventosVivos.Domain.Tests;

public class ReservationTests
{
    [Fact]
    public void Create_WithValidData_IsPendingPayment()
    {
        var r = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2, "Ana", "ana@example.com");

        Assert.NotEqual(Guid.Empty, r.Id);
        Assert.Equal(ReservationStatus.PendingPayment, r.Status);
        Assert.Null(r.ReservationCode);
        Assert.False(r.IsLost);
    }

    [Fact]
    public void Create_QuantityLessThanOne_Throws()
    {
        var ex = Assert.Throws<InvalidEventException>(
            () => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 0, "Ana", "ana@example.com"));
        Assert.Equal("INVALID_QUANTITY", ex.Code);
    }

    [Fact]
    public void Create_EmptyEmail_Throws()
    {
        var ex = Assert.Throws<InvalidEventException>(
            () => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "Ana", "  "));
        Assert.Equal("INVALID_BUYER_EMAIL", ex.Code);
    }

    [Fact]
    public void ConfirmPayment_FromPending_Confirms()
    {
        var r = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "Ana", "ana@example.com");

        r.ConfirmPayment("EV-123456");

        Assert.Equal(ReservationStatus.Confirmed, r.Status);
        Assert.Equal("EV-123456", r.ReservationCode);
    }

    [Fact]
    public void ConfirmPayment_AlreadyConfirmed_Throws()
    {
        var r = TestData.ConfirmedReservation();

        var ex = Assert.Throws<InvalidReservationStateException>(() => r.ConfirmPayment("EV-999999"));
        Assert.Equal("RESERVATION_ALREADY_CONFIRMED", ex.Code);
    }

    [Fact]
    public void ConfirmPayment_Cancelled_Throws()
    {
        var r = TestData.ConfirmedReservation();
        r.Cancel(DateTime.UtcNow.AddMonths(1));

        var ex = Assert.Throws<InvalidReservationStateException>(() => r.ConfirmPayment("EV-999999"));
        Assert.Equal("RESERVATION_CANCELLED", ex.Code);
    }

    [Fact]
    public void Cancel_PendingPayment_Throws()
    {
        var r = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "Ana", "ana@example.com");

        var ex = Assert.Throws<InvalidReservationStateException>(() => r.Cancel(DateTime.UtcNow.AddMonths(1)));
        Assert.Equal("INVALID_STATE", ex.Code);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var r = TestData.ConfirmedReservation();
        r.Cancel(DateTime.UtcNow.AddMonths(1));

        var ex = Assert.Throws<InvalidReservationStateException>(() => r.Cancel(DateTime.UtcNow.AddMonths(1)));
        Assert.Equal("RESERVATION_ALREADY_CANCELLED", ex.Code);
    }

    [Fact]
    public void Cancel_Outside48hWindow_IsNotLost()
    {
        var r = TestData.ConfirmedReservation();

        r.Cancel(DateTime.UtcNow.AddHours(72));

        Assert.Equal(ReservationStatus.Cancelled, r.Status);
        Assert.False(r.IsLost);
        Assert.NotNull(r.CancellationDate);
    }

    [Fact]
    public void Cancel_Within48hWindow_IsLost()
    {
        var r = TestData.ConfirmedReservation();

        r.Cancel(DateTime.UtcNow.AddHours(24));

        Assert.Equal(ReservationStatus.Cancelled, r.Status);
        Assert.True(r.IsLost);
    }
}
