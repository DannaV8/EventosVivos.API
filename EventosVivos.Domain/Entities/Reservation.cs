using System.Text.RegularExpressions;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.Domain.Entities;

public sealed class Reservation
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
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
                "Event identifier is required.");

        if (userId == Guid.Empty)
            throw new InvalidEventException("INVALID_USER",
                "User identifier is required.");

        if (quantity < 1)
            throw new InvalidEventException("INVALID_QUANTITY",
                "Quantity must be at least 1.");

        if (string.IsNullOrWhiteSpace(buyerName))
            throw new InvalidEventException("INVALID_BUYER_NAME",
                "Buyer name is required.");

        if (string.IsNullOrWhiteSpace(buyerEmail) || !EmailPattern.IsMatch(buyerEmail))
            throw new InvalidEventException("INVALID_BUYER_EMAIL",
                "A valid buyer email is required.");

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

    private static readonly Regex CodePattern = new(@"^EV-\d{6}$", RegexOptions.Compiled);

    public void ConfirmPayment(string reservationCode)
    {
        if (!CodePattern.IsMatch(reservationCode))
            throw new InvalidReservationStateException("INVALID_CODE_FORMAT",
                "Reservation code must match EV-{6 digits}.");
        if (Status == ReservationStatus.Confirmed)
            throw new InvalidReservationStateException("RESERVATION_ALREADY_CONFIRMED",
                "Reservation is already confirmed.");
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidReservationStateException("RESERVATION_CANCELLED",
                "Cannot confirm a cancelled reservation.");

        Status = ReservationStatus.Confirmed;
        ReservationCode = reservationCode;
    }

    public void Cancel(DateTime eventStart)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidReservationStateException("RESERVATION_ALREADY_CANCELLED",
                "Reservation is already cancelled.");
        if (Status == ReservationStatus.PendingPayment)
            throw new InvalidReservationStateException("INVALID_STATE",
                "Cannot cancel a pending payment reservation.");

        IsLost = DateTime.UtcNow > eventStart.AddHours(-48);
        Status = ReservationStatus.Cancelled;
        CancellationDate = DateTime.UtcNow;
    }
}
