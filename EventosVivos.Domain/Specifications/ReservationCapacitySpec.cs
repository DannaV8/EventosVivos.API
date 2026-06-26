namespace EventosVivos.Domain.Specifications;
public sealed class ReservationCapacitySpec
{
    public (bool isValid, string? errorCode, string? errorMessage) Evaluate(
        int quantity,
        int maxCapacity,
        int confirmedTickets,
        int lostTickets)
    {
        var available = maxCapacity - confirmedTickets - lostTickets;

        if (quantity > available)
            return (
                false,
                "CAPACITY_EXCEEDED",
                $"Not enough tickets. Requested: {quantity}, Available: {available}");

        return (true, null, null);
    }
}
