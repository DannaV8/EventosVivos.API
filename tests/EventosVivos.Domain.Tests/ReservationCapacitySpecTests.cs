using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Specifications;
using Xunit;

namespace EventosVivos.Domain.Tests;

public class ReservationCapacitySpecTests
{
    private readonly ReservationCapacitySpec _spec = new();

    [Fact]
    public void Evaluate_WithinCapacity_IsValid()
    {
        var (isValid, code, _) = _spec.Evaluate(quantity: 10, maxCapacity: 100,
            confirmedTickets: 30, lostTickets: 5);

        Assert.True(isValid);
        Assert.Null(code);
    }

    [Fact]
    public void Evaluate_QuantityEqualsAvailable_IsValid()
    {
        var (isValid, _, _) = _spec.Evaluate(10, 100, 90, 0);
        Assert.True(isValid);
    }

    [Fact]
    public void Evaluate_ExceedsCapacity_IsInvalid()
    {
        var (isValid, code, _) = _spec.Evaluate(3, 100, 95, 3);

        Assert.False(isValid);
        Assert.Equal("CAPACITY_EXCEEDED", code);
    }

    [Fact]
    public void Evaluate_LostTicketsReduceAvailability()
    {
        var (isValid, code, _) = _spec.Evaluate(1, 50, 0, 50);

        Assert.False(isValid);
        Assert.Equal("CAPACITY_EXCEEDED", code);
    }
}

public class ReservationTransactionLimitSpecTests
{
    private readonly ReservationTransactionLimitSpec _spec = new();

    [Fact]
    public void Evaluate_ZeroQuantity_IsInvalid()
    {
        var (isValid, code, _) = _spec.Evaluate(0, 50m, DateTime.UtcNow.AddDays(10));

        Assert.False(isValid);
        Assert.Equal("INVALID_QUANTITY", code);
    }

    [Fact]
    public void Evaluate_LessThan24h_MoreThan5_IsInvalid()
    {
        var (isValid, code, _) = _spec.Evaluate(6, 50m, DateTime.UtcNow.AddHours(12));

        Assert.False(isValid);
        Assert.Equal("LIMIT_24H", code);
    }

    [Fact]
    public void Evaluate_LessThan24h_Exactly5_IsValid()
    {
        var (isValid, _, _) = _spec.Evaluate(5, 50m, DateTime.UtcNow.AddHours(12));
        Assert.True(isValid);
    }

    [Fact]
    public void Evaluate_HighPrice_MoreThan10_IsInvalid()
    {
        var (isValid, code, _) = _spec.Evaluate(11, 150m, DateTime.UtcNow.AddDays(10));

        Assert.False(isValid);
        Assert.Equal("PRICE_LIMIT", code);
    }

    [Fact]
    public void Evaluate_HighPrice_Exactly10_IsValid()
    {
        var (isValid, _, _) = _spec.Evaluate(10, 150m, DateTime.UtcNow.AddDays(10));
        Assert.True(isValid);
    }

    [Fact]
    public void Evaluate_24hRuleHasPriorityOverPriceRule()
    {
        var (isValid, code, _) = _spec.Evaluate(8, 150m, DateTime.UtcNow.AddHours(10));

        Assert.False(isValid);
        Assert.Equal("LIMIT_24H", code);
    }

    [Fact]
    public void Evaluate_LowPriceFarFuture_IsValid()
    {
        var (isValid, _, _) = _spec.Evaluate(50, 20m, DateTime.UtcNow.AddDays(30));
        Assert.True(isValid);
    }
}

public class VenueAvailabilitySpecTests
{
    private readonly VenueAvailabilitySpec _spec = new();

    [Fact]
    public void HasConflict_OverlappingRanges_ReturnsTrue()
    {
        var start = TestData.NextWeekday(18);
        var existing = Domain.Entities.Event.Create(
            "Existente", "Evento que ya ocupa el venue.", TestData.Venue(),
            100, start, start.AddHours(3), 50m, EventType.Concert);

        var hasConflict = _spec.HasConflict(
            venueId: 1, start.AddHours(1), start.AddHours(2),
            new[] { existing });

        Assert.True(hasConflict);
    }

    [Fact]
    public void HasConflict_AdjacentRanges_ReturnsFalse()
    {
        var start = TestData.NextWeekday(18);
        var existing = Domain.Entities.Event.Create(
            "Existente", "Evento que ya ocupa el venue.", TestData.Venue(),
            100, start, start.AddHours(3), 50m, EventType.Concert);

        var hasConflict = _spec.HasConflict(
            venueId: 1, start.AddHours(3), start.AddHours(5),
            new[] { existing });

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflict_DifferentVenue_ReturnsFalse()
    {
        var start = TestData.NextWeekday(18);
        var existing = Domain.Entities.Event.Create(
            "Existente", "Evento que ya ocupa el venue.", TestData.Venue(),
            100, start, start.AddHours(3), 50m, EventType.Concert);

        var hasConflict = _spec.HasConflict(
            venueId: 99, start.AddHours(1), start.AddHours(2),
            new[] { existing });

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflict_CancelledEvent_IsIgnored()
    {
        var start = TestData.NextWeekday(18);
        var existing = Domain.Entities.Event.Create(
            "Existente", "Evento que ya ocupa el venue.", TestData.Venue(),
            100, start, start.AddHours(3), 50m, EventType.Concert);
        existing.Cancel();

        var hasConflict = _spec.HasConflict(
            venueId: 1, start.AddHours(1), start.AddHours(2),
            new[] { existing });

        Assert.False(hasConflict);
    }

    [Fact]
    public void HasConflict_ExcludedEventId_ReturnsFalse()
    {
        var start = TestData.NextWeekday(18);
        var existing = Domain.Entities.Event.Create(
            "Existente", "Evento que ya ocupa el venue.", TestData.Venue(),
            100, start, start.AddHours(3), 50m, EventType.Concert);

        var hasConflict = _spec.HasConflict(
            venueId: 1, start.AddHours(1), start.AddHours(2),
            new[] { existing }, excludeEventId: existing.Id);

        Assert.False(hasConflict);
    }
}
