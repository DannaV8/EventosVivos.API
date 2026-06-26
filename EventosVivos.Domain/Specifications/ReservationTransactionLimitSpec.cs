namespace EventosVivos.Domain.Specifications;
public sealed class ReservationTransactionLimitSpec
{
    public (bool isValid, string? errorCode, string? errorMessage) Evaluate(
     int quantity,
     decimal price,
     DateTime eventStart)
    {
        var hoursUntilEvent = (eventStart - DateTime.UtcNow).TotalHours;

        if (hoursUntilEvent < 24 && quantity > 5)
            return (false, "LIMIT_24H", "Less than 24h until event: maximum 5 tickets.");

        if (price > 100 && quantity > 10)
            return (false, "PRICE_LIMIT", "Events with price > $100: maximum 10 tickets.");

        return (true, null, null);
    }
}
