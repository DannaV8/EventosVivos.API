using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Specifications;

namespace EventosVivos.Domain.Services;

public sealed class ReservationValidationService
{
    private readonly ReservationCapacitySpec _capacitySpec = new();
    private readonly ReservationTransactionLimitSpec _limitSpec = new();

    public void Validate(
        EventStatus eventStatus,
        int quantity,
        decimal ticketPrice,
        DateTime eventStart,
        int maxCapacity,
        int confirmedTickets,
        int lostTickets)
    {
        if (eventStatus != EventStatus.Active)
            throw new InvalidEventException("EVENT_NOT_ACTIVE",
                "Reservations can only be created for active events.");

        if ((eventStart - DateTime.UtcNow).TotalHours < 1)
            throw new EventSoonException();

        var (isCapacityValid, _, _) = _capacitySpec.Evaluate(
            quantity, maxCapacity, confirmedTickets, lostTickets);
        if (!isCapacityValid)
        {
            var available = maxCapacity - confirmedTickets - lostTickets;
            throw new CapacityExceededException(quantity, available);
        }

        var (isLimitValid, errorCode, errorMessage) = _limitSpec.Evaluate(
            quantity, ticketPrice, eventStart);
        if (!isLimitValid)
            throw new InvalidEventException(errorCode!, errorMessage!);

        if (quantity < 1)
            throw new InvalidEventException("INVALID_QUANTITY",
                "Quantity must be at least 1.");
    }
}
