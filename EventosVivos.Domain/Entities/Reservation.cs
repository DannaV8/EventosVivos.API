using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public int Quantity { get; private set; }
    public string BuyerName { get; private set; } = null!;
    public string BuyerEmail { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public string? ReservationCode { get; private set; }
    public bool IsLost { get; private set; }
    public DateTime? CancellationDate { get; private set; }
    public DateTime CreationDate { get; private set; }

    // Constructor for EF Core.
    private Reservation() { }

    public static Reservation Create(
        Guid eventId, Guid userId, int quantity,
        string buyerName, string buyerEmail)
    {
        if (eventId == Guid.Empty)
            throw new InvalidEventException("INVALID_EVENT",
                "El identificador del evento es obligatorio.");

        if (userId == Guid.Empty)
            throw new InvalidEventException("INVALID_USER",
                "El identificador del usuario es obligatorio.");

        if (quantity < 1)
            throw new InvalidEventException("INVALID_QUANTITY",
                "La cantidad debe ser al menos 1.");

        if (string.IsNullOrWhiteSpace(buyerName))
            throw new InvalidEventException("INVALID_BUYER_NAME",
                "El nombre del comprador es obligatorio.");

        if (string.IsNullOrWhiteSpace(buyerEmail))
            throw new InvalidEventException("INVALID_BUYER_EMAIL",
                "El email del comprador es obligatorio.");

        return new Reservation
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Quantity = quantity,
            BuyerName = buyerName,
            BuyerEmail = buyerEmail,
            Status = ReservationStatus.PendingPayment,
            ReservationCode = null,
            IsLost = false,
            CreationDate = DateTime.UtcNow
        };
    }

    public void ConfirmPayment(string reservationCode)
    {
        if (Status == ReservationStatus.Confirmed)
            throw new InvalidReservationStateException("RESERVATION_ALREADY_CONFIRMED",
                "La reserva ya está confirmada.");
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidReservationStateException("RESERVATION_CANCELLED",
                "No se puede confirmar una reserva cancelada.");

        Status = ReservationStatus.Confirmed;
        ReservationCode = reservationCode;
    }

    public void Cancel(DateTime eventStart)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidReservationStateException("RESERVATION_ALREADY_CANCELLED",
                "La reserva ya está cancelada.");
        if (Status == ReservationStatus.PendingPayment)
            throw new InvalidReservationStateException("INVALID_STATE",
                "No se puede cancelar una reserva pendiente de pago.");

        IsLost = DateTime.UtcNow > eventStart.AddHours(-48);
        Status = ReservationStatus.Cancelled;
        CancellationDate = DateTime.UtcNow;
    }
}
