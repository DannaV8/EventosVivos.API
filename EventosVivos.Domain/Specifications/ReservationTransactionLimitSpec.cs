namespace EventosVivos.Domain.Specifications;
public sealed class ReservationTransactionLimitSpec
{
    public (bool isValid, string? errorCode, string? errorMessage) Evaluate(
     int quantity,
     decimal price,
     DateTime eventStart)
    {
        if (quantity < 1)
            return (false, "INVALID_QUANTITY", "La cantidad debe ser al menos 1.");

        var hoursUntilEvent = (eventStart - DateTime.UtcNow).TotalHours;

        if (hoursUntilEvent < 24 && quantity > 5)
            return (false, "LIMIT_24H", "Menos de 24h para el evento: máximo 5 entradas.");

        if (price > 100 && quantity > 10)
            return (false, "PRICE_LIMIT", "Eventos con precio > $100: máximo 10 entradas.");

        return (true, null, null);
    }
}
