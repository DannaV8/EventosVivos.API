using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Services;
using Xunit;

namespace EventosVivos.Domain.Tests;

public class ReservationValidationServiceTests
{
    private readonly ReservationValidationService _service = new();

    [Fact]
    public void Validate_AllValid_DoesNotThrow()
    {
        var start = DateTime.UtcNow.AddDays(10);

        _service.Validate(
            quantity: 3, ticketPrice: 50m, eventStart: start,
            maxCapacity: 100, confirmedTickets: 10, lostTickets: 0);
    }

    [Fact]
    public void Validate_EventStartsInLessThan1h_ThrowsEventSoon()
    {
        var start = DateTime.UtcNow.AddMinutes(30);

        var ex = Assert.Throws<EventSoonException>(() => _service.Validate(
            quantity: 1, ticketPrice: 50m, eventStart: start,
            maxCapacity: 100, confirmedTickets: 0, lostTickets: 0));
        Assert.Equal("EVENT_SOON", ex.Code);
    }

    [Fact]
    public void Validate_RN04HasPriorityOverCapacity()
    {
        var start = DateTime.UtcNow.AddMinutes(30);

        var ex = Assert.Throws<EventSoonException>(() => _service.Validate(
            quantity: 999, ticketPrice: 50m, eventStart: start,
            maxCapacity: 10, confirmedTickets: 10, lostTickets: 0));
        Assert.Equal("EVENT_SOON", ex.Code);
    }

    [Fact]
    public void Validate_NoCapacity_ThrowsCapacityExceeded()
    {
        var start = DateTime.UtcNow.AddDays(10);

        var ex = Assert.Throws<CapacityExceededException>(() => _service.Validate(
            quantity: 5, ticketPrice: 50m, eventStart: start,
            maxCapacity: 100, confirmedTickets: 98, lostTickets: 0));
        Assert.Equal("CAPACITY_EXCEEDED", ex.Code);
    }

    [Fact]
    public void Validate_CapacityHasPriorityOverLimit()
    {
        var start = DateTime.UtcNow.AddDays(10);

        var ex = Assert.Throws<CapacityExceededException>(() => _service.Validate(
            quantity: 11, ticketPrice: 150m, eventStart: start,
            maxCapacity: 100, confirmedTickets: 95, lostTickets: 0));
        Assert.Equal("CAPACITY_EXCEEDED", ex.Code);
    }

    [Fact]
    public void Validate_ExceedsPriceLimit_ThrowsInvalidEvent()
    {
        var start = DateTime.UtcNow.AddDays(10);

        var ex = Assert.Throws<InvalidEventException>(() => _service.Validate(
            quantity: 11, ticketPrice: 150m, eventStart: start,
            maxCapacity: 500, confirmedTickets: 0, lostTickets: 0));
        Assert.Equal("PRICE_LIMIT", ex.Code);
    }

    [Fact]
    public void Validate_LessThan24h_ExceedsLimit_Throws24hLimit()
    {
        var start = DateTime.UtcNow.AddHours(12);

        var ex = Assert.Throws<InvalidEventException>(() => _service.Validate(
            quantity: 6, ticketPrice: 50m, eventStart: start,
            maxCapacity: 500, confirmedTickets: 0, lostTickets: 0));
        Assert.Equal("LIMIT_24H", ex.Code);
    }
}
